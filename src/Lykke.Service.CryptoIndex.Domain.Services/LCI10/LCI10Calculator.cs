using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.CryptoIndex.Domain.Models;
using Lykke.Service.CryptoIndex.Domain.Models.LCI10;
using Lykke.Service.CryptoIndex.Domain.Publishers;
using Lykke.Service.CryptoIndex.Domain.Repositories.LCI10;

namespace Lykke.Service.CryptoIndex.Domain.Services.LCI10
{
    /// <summary>
    /// See the specification - https://lykkex.atlassian.net/secure/attachment/46308/LCI_specs.pdf
    /// </summary>
    public class LCI10Calculator : ILCI10Calculator, IStartable, IStopable
    {
        private const decimal InitialIndexValue = 1000m;
        private const string Lci10 = "lci10";
        private static TimeSpan _waitForTopAssetsPricesFromStart = TimeSpan.FromMinutes(2);
        private readonly DateTime _startedAt;
        private readonly object _sync = new object();
        private readonly List<AssetMarketCap> _topMarketCaps;
        private readonly IDictionary<string, decimal> _topAssetsWeights;
        private readonly TimerTrigger _weightsCalculationTrigger;
        private readonly TimerTrigger _indexCalculationTrigger;
        private readonly ILog _log;

        private IIndexStateRepository IndexStateRepository { get; set; }
        private IIndexHistoryRepository IndexHistoryRepository { get; set; }
        private IWarningRepository WarningRepository { get; set; }
        private ISettingsService SettingsService { get; set; }
        private ITickPricesService TickPricesService { get; set; }
        private ITickPricePublisher TickPricePublisher { get; set; }
        private IMarketCapitalizationService MarketCapitalizationService { get; set; }

        private Settings Settings => SettingsService.GetAsync().GetAwaiter().GetResult();
        private IndexState State => IndexStateRepository.GetAsync().GetAwaiter().GetResult();

        public LCI10Calculator(TimeSpan weightsCalculationInterval, TimeSpan indexCalculationInterval, ILogFactory logFactory)
        {
            _startedAt = DateTime.UtcNow;
            _topMarketCaps = new List<AssetMarketCap>();
            _topAssetsWeights = new ConcurrentDictionary<string, decimal>();

            _weightsCalculationTrigger = new TimerTrigger(nameof(LCI10Calculator), weightsCalculationInterval, logFactory, CalculateWeights);
            _indexCalculationTrigger = new TimerTrigger(nameof(LCI10Calculator), indexCalculationInterval, logFactory, CalculateIndex);

            _log = logFactory.CreateLog(this);
        }

        public async Task<IReadOnlyDictionary<string, decimal>> GetTopAssetsMarketCapsAsync()
        {
            var result = new Dictionary<string, decimal>();

            lock (_sync)
            {
                foreach (var x in _topMarketCaps)
                    result.Add(x.Asset, x.MarketCap.Value);
            }

            return result;
        }

        public Task Reset()
        {
            IndexStateRepository.Clear().GetAwaiter().GetResult();

            return Task.CompletedTask;
        }

        private async Task CalculateWeights(ITimerTrigger timer, TimerTriggeredHandlerArgs args, CancellationToken ct)
        {
            try
            {
                await CalculateWeights();
            }
            catch (Exception e)
            {
                _log.Warning($"Something went wrong while calculating weights.", e);
            }
        }

        private async Task CalculateWeights()
        {
            _log.Info("Calculating weights...");

            // Get top 100 market caps
            var allMarketCaps = await MarketCapitalizationService.GetAllAsync();

            // Top wanted market caps
            var topMarketCaps = allMarketCaps.Where(x => Settings.Assets.Contains(x.Asset))
                .OrderByDescending(x => x.MarketCap.Value)
                .Take(Settings.TopCount)
                .ToList();

            // Sum of top market caps
            var totalMarketCap = topMarketCaps.Select(x => x.MarketCap.Value).Sum();

            // Calculate weights
            var wantedAssets = topMarketCaps.Select(x => x.Asset).ToList();
            var topAssetsWeights = new Dictionary<string, decimal>();
            foreach (var asset in wantedAssets)
            {
                var assetMarketCap = topMarketCaps.Single(x => x.Asset == asset).MarketCap.Value;
                var assetWeight = assetMarketCap / totalMarketCap;
                topAssetsWeights[asset] = assetWeight;
            }

            lock (_sync)
            {
                // Refresh market caps
                _topMarketCaps.Clear();
                _topMarketCaps.AddRange(topMarketCaps);

                // Refresh weights
                _topAssetsWeights.Clear();
                foreach (var assetWeight in topAssetsWeights)
                    _topAssetsWeights.Add(assetWeight);
            }

            _log.Info($"Finished calculating weights for {wantedAssets.Count} assets.");
        }

        private async Task CalculateIndex(ITimerTrigger timer, TimerTriggeredHandlerArgs args, CancellationToken ct)
        {
            try
            {
                if (!Settings.Enabled)
                    return;

                await CalculateIndex();
            }
            catch (Exception e)
            {
                _log.Warning("Somethings went wrong while index calculation.", e);
            }
        }

        private async Task CalculateIndex()
        {
            _log.Info("Started calculating index...");

            List<AssetMarketCap> topMarketCaps;
            IDictionary<string, decimal> topAssetWeights;
            var allAssetsPrices = await TickPricesService.GetPricesAsync();
            var lastIndex = await IndexStateRepository.GetAsync();

            lock (_sync)
            {
                topMarketCaps = _topMarketCaps.ToList();
                topAssetWeights = _topAssetsWeights.Clone();
            }

            // Get raw prices for the top assets
            var topAssets = topAssetWeights.Keys.ToList();
            var topAssetsPrices = GetTopAssetsPrices(allAssetsPrices, topAssets);

            // If just started and prices not present yet, then skip.
            // If started more then {_waitForTopAssetsPricesFromStart} ago then write a warning to the DB and log.
            if (!AllPricesArePresentForTopAssets(topAssetWeights, topAssetsPrices))
                return;

            // Get current index state
            var indexState = CalculateIndexState(topAssetWeights, topAssetsPrices, lastIndex);

            // if there was a reset then skip until next iteration which will have initial state
            if (indexState.Value != InitialIndexValue && State == null)
                return;

            // Skip if changed to 'disabled'
            if (!Settings.Enabled)
                return;

            // Save index state for the next execution
            await IndexStateRepository.SetAsync(indexState);

            // Save all index info to history
            var indexHistory = new IndexHistory(indexState.Value, topMarketCaps, topAssetWeights, topAssetsPrices, indexState.MiddlePrices, DateTime.UtcNow);
            await IndexHistoryRepository.InsertAsync(indexHistory);

            // Publish index to RabbitMq
            var tickPrice = new Models.TickPrice(Lci10, Lci10, indexHistory.Value, indexHistory.Value, indexHistory.Time);
            TickPricePublisher.Publish(tickPrice);

            _log.Info($"Finished calculating index for {topAssets.Count} assets, value: {indexState.Value}.");
        }

        private IndexState CalculateIndexState(IDictionary<string, decimal> topAssetWeights,
            IDictionary<string, IDictionary<string, decimal>> topAssetsPrices, IndexState lastIndex)
        {
            var topMiddlePrices = GetMiddlePrices(topAssetsPrices);

            if (lastIndex == null)
                lastIndex = new IndexState(InitialIndexValue, topMiddlePrices);

            var rLci10 = 0m;

            foreach (var asset in topAssetWeights.Keys.ToList())
            {
                var middlePrice = GetMiddlePrice(asset, topAssetsPrices);
                var previousMiddlePrice = GetPreviousMiddlePrice(asset, lastIndex.MiddlePrices, middlePrice);

                var weight = topAssetWeights[asset];

                var r = middlePrice / previousMiddlePrice;

                rLci10 += weight * r;
            }

            var index = Math.Round(lastIndex.Value * rLci10, 2);

            var indexState = new IndexState(index, topMiddlePrices);

            return indexState;
        }

        private IDictionary<string, IDictionary<string, decimal>> GetTopAssetsPrices(IDictionary<string, IDictionary<string, decimal>> allAssetsPrices, IReadOnlyList<string> topAssets)
        {
            var result = new Dictionary<string, IDictionary<string, decimal>>();
            foreach (var asset in topAssets)
                if (allAssetsPrices.ContainsKey(asset))
                    result[asset] = allAssetsPrices[asset];

            return result;
        }

        private bool AllPricesArePresentForTopAssets(IDictionary<string, decimal> topAssetWeights,
            IDictionary<string, IDictionary<string, decimal>> topAssetsPrices)
        {
            var assetsWoPrices = topAssetWeights.Keys.Where(x => !topAssetsPrices.Keys.Contains(x)).ToList();

            if (assetsWoPrices.Any())
            {
                var message = $"Some assets don't have prices: {assetsWoPrices.ToJson()}.";

                // If just started then skip current iteration
                if (DateTime.UtcNow - _startedAt > _waitForTopAssetsPricesFromStart)
                {
                    WarningRepository.SaveAsync(new Warning(message, DateTime.UtcNow));
                    throw new InvalidOperationException(message);
                }
                
                return false;
            }

            return true;
        }

        private static decimal GetMiddlePrice(string asset, IDictionary<string, IDictionary<string, decimal>> assetsPrices)
        {
            if (!assetsPrices.ContainsKey(asset))
                throw new InvalidOperationException($"Asset '{asset}' is not found in prices: {assetsPrices.ToJson()}.");

            var prices = assetsPrices[asset].Values.ToList();

            var middlePrice = prices.Sum() / prices.Count;

            return middlePrice;
        }

        private static IDictionary<string, decimal> GetMiddlePrices(IDictionary<string, IDictionary<string, decimal>> assetsPrices)
        {
            var result = new Dictionary<string, decimal>();

            foreach (var asset in assetsPrices.Keys)
            {
                result.Add(asset, GetMiddlePrice(asset, assetsPrices));
            }

            return result;
        }

        private static decimal GetPreviousMiddlePrice(string asset, IDictionary<string, decimal> previousPrices, decimal currentMiddlePrice)
        {
            return previousPrices.ContainsKey(asset)  // previous prices found in DB?
                ? previousPrices[asset]               // yes, use them
                : currentMiddlePrice;                 // no, use current
        }

        public void Start()
        {
            _weightsCalculationTrigger.Start();
            _indexCalculationTrigger.Start();
        }

        public void Stop()
        {
            _weightsCalculationTrigger.Stop();
            _indexCalculationTrigger.Stop();
        }

        public void Dispose()
        {
            MarketCapitalizationService?.Dispose();
            _weightsCalculationTrigger?.Dispose();
        }
    }
}
