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
using MoreLinq;

namespace Lykke.Service.CryptoIndex.Domain.Services.LCI10
{
    /// <summary>
    /// See the specification - https://lykkex.atlassian.net/secure/attachment/46308/LCI_specs.pdf
    /// </summary>
    public class LCI10Calculator : ILCI10Calculator, IStartable, IStopable
    {
        private const decimal InitialIndexValue = 1000m;
        private const string Lci10 = "lci10";
        private readonly object _sync = new object();
        private readonly List<AssetMarketCap> _marketCaps;
        private readonly IDictionary<string, decimal> _allMarketCapsAssets;
        private readonly IDictionary<string, decimal> _assetsWeights;
        private readonly IIndexStateRepository _indexStateRepository;
        private readonly IIndexHistoryRepository _indexHistoryRepository;
        private readonly IWarningRepository _warningRepository;
        private readonly ISettingsService _settingsService;
        private readonly ITickPricesService _tickPricesService;
        private readonly ITickPricePublisher _tickPricePublisher;
        private readonly IMarketCapitalizationService _marketCapitalizationService;
        private readonly TimerTrigger _weightsCalculationTrigger;
        private readonly TimerTrigger _indexCalculationTrigger;
        private readonly ILog _log;

        private Settings Settings => _settingsService.GetAsync().GetAwaiter().GetResult();
        private IndexState State => _indexStateRepository.GetAsync().GetAwaiter().GetResult();
        private IReadOnlyList<string> TopAssets => _assetsWeights.Keys.ToList();

        public LCI10Calculator(IIndexStateRepository indexStateRepository, IIndexHistoryRepository indexHistoryRepository, IWarningRepository warningRepository,
            ISettingsService settingsService, ITickPricePublisher tickPricePublisher, IMarketCapitalizationService marketCapitalizationService,
            TimeSpan weightsCalculationInterval, TimeSpan indexCalculationInterval, ITickPricesService tickPricesService, ILogFactory logFactory)
        {
            _marketCaps = new List<AssetMarketCap>();
            _allMarketCapsAssets = new Dictionary<string, decimal>();
            _assetsWeights = new ConcurrentDictionary<string, decimal>();

            _indexStateRepository = indexStateRepository ?? throw new ArgumentNullException(nameof(indexStateRepository));
            _indexHistoryRepository = indexHistoryRepository ?? throw new ArgumentNullException(nameof(_indexHistoryRepository));
            _warningRepository = warningRepository ?? throw new ArgumentNullException(nameof(_warningRepository));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _marketCapitalizationService = marketCapitalizationService ?? throw new ArgumentNullException(nameof(marketCapitalizationService));
            _tickPricePublisher = tickPricePublisher ?? throw new ArgumentNullException(nameof(tickPricePublisher));
            _tickPricesService = tickPricesService;

            _weightsCalculationTrigger = new TimerTrigger(nameof(LCI10Calculator), weightsCalculationInterval, logFactory, CalculateWeights);
            _indexCalculationTrigger = new TimerTrigger(nameof(LCI10Calculator), indexCalculationInterval, logFactory, CalculateIndex);

            _log = logFactory.CreateLog(this);
        }

        public async Task<IReadOnlyDictionary<string, decimal>> GetAssetsMarketCapAsync()
        {
            var result = new Dictionary<string, decimal>();

            _marketCaps.Take(Settings.TopCount*2)
                       .ForEach(x => result.Add(x.Asset, x.MarketCap.Value));

            return result;
        }

        public Task Reset()
        {
            lock (_sync)
            {
                _indexStateRepository.Clear().GetAwaiter().GetResult();
            }

            return Task.CompletedTask;
        }

        private async Task CalculateWeights(ITimerTrigger timer, TimerTriggeredHandlerArgs args, CancellationToken ct)
        {
            try
            {
                if (!Settings.Enabled)
                    return;

                await CalculateWeights();
            }
            catch (Exception e)
            {
                _log.Warning($"Something went wrong while calculating weights.", e);
            }
        }

        private async Task CalculateWeights()
        {
            _log.Info("Started calculating weights...");

            // Get top 100 market caps
            var all = await _marketCapitalizationService.GetAllAsync();

            // All market caps assets
            all.ForEach(x => _allMarketCapsAssets.Add(x.Asset, x.MarketCap.Value));

            // Top wanted market caps
            var topMarketCaps = GetTopMarketCaps(all);

            // Sum of top market caps
            var totalMarketCap = topMarketCaps.Select(x => x.MarketCap.Value).Sum();

            lock (_sync)
            {
                // Refresh market caps
                _marketCaps.Clear();
                _marketCaps.AddRange(topMarketCaps);

                // Calculate weights
                _assetsWeights.Clear();
                foreach (var asset in TopAssets)
                {
                    var assetMarketCap = topMarketCaps.Single(x => x.Asset == asset).MarketCap.Value;
                    var assetWeight = assetMarketCap / totalMarketCap;
                    _assetsWeights[asset] = assetWeight;
                }
            }

            _log.Info($"Finished calculating weights for {TopAssets.Count} assets.");
        }

        private IReadOnlyList<AssetMarketCap> GetTopMarketCaps(IEnumerable<AssetMarketCap> all)
        {
            // Exclude not wanted assets
            var wantedOnly = all.Where(x => !Settings.ExcludedAssets.Contains(x.Asset));

            // Get top wanted market caps
            var result = wantedOnly.OrderByDescending(x => x.MarketCap.Value)
                .Take(Settings.TopCount).ToList();

            return result;
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

            List<AssetMarketCap> marketCaps;
            IDictionary<string, decimal> assetWeights;
            var assetsPrices = await _tickPricesService.GetPricesAsync();
            var lastIndex = await _indexStateRepository.GetAsync();

            lock (_sync)
            {
                marketCaps = _marketCaps.ToList();
                assetWeights = _assetsWeights.Clone();
            }

            if (!IsAllDataPresent(marketCaps, assetWeights, assetsPrices))
            {
                _log.Info("Skipped LCI10 calculation.");
                return;
            }

            lock (_sync)
            {
                if (!Settings.Enabled)
                {
                    return;
                }       
            }

            // Get current index state
            var indexState = CalculateIndexState(assetWeights, assetsPrices, lastIndex);

            // if there was a reset then skip until next iteration which will have initial state
            if (indexState.Value != InitialIndexValue && State == null)
                return;

            // Save index state for the next execution
            await _indexStateRepository.SetAsync(indexState);

            // Save all index info to history
            var indexHistory = new IndexHistory(indexState.Value, marketCaps, assetWeights, assetsPrices, indexState.MiddlePrices, DateTime.UtcNow);
            await _indexHistoryRepository.InsertAsync(indexHistory);

            // Publish index to RabbitMq
            var tickPrice = new Models.TickPrice(Lci10, Lci10, indexHistory.Value, indexHistory.Value, indexHistory.Time);
            _tickPricePublisher.Publish(tickPrice);

            _log.Info($"Finished calculating index for {TopAssets.Count} assets, value: {indexState.Value}.");
        }

        private IndexState CalculateIndexState(IDictionary<string, decimal> assetsWeights,
            IDictionary<string, IDictionary<string, decimal>> assetsPrices, IndexState lastIndex)
        {
            var middlePrices = GetMiddlePrices(assetsPrices);

            if (lastIndex == null)
                lastIndex = new IndexState(InitialIndexValue, middlePrices);

            CheckThatAllAssetsArePresent(assetsWeights, assetsPrices);

            var rLci10 = 0m;

            foreach (var asset in TopAssets)
            {
                var middlePrice = GetMiddlePrice(asset, assetsPrices);
                var previousMiddlePrice = GetPreviousMiddlePrice(asset, lastIndex.MiddlePrices, middlePrice);

                var weight = assetsWeights[asset];

                var r = middlePrice / previousMiddlePrice;

                rLci10 += weight * r;
            }

            var index = Math.Round(lastIndex.Value * rLci10, 2);

            var indexState = new IndexState(index, middlePrices);

            return indexState;
        }

        private void CheckThatAllAssetsArePresent(IDictionary<string, decimal> assetsWeights,
            IDictionary<string, IDictionary<string, decimal>> assetsPrices)
        {
            if (assetsWeights.Count != assetsPrices.Count)
            {
                var message = $"Some assets are missed, weights: {assetsWeights.Keys.ToJson()}, prices: {assetsPrices.Keys.ToJson()}.";
                _warningRepository.SaveAsync(new Warning(message, DateTime.UtcNow));
                throw new InvalidOperationException(message);
            }    
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

        private bool IsAllDataPresent(IEnumerable<AssetMarketCap> marketCaps, IDictionary<string, decimal> weights,
            IDictionary<string, IDictionary<string, decimal>> prices)
        {
            if (!marketCaps.Any())
            {
                _log.Info("Market Cap data is not filled yet.");
                return false;
            }

            if (!weights.Any())
            {
                _log.Info("Assets Weights is not calculated yet.");
                return false;
            }

            if (!prices.Any())
            {
                _log.Info("Assets Prices cache is not filled yet.");
                return false;
            }

            return true;
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
            _marketCapitalizationService?.Dispose();
            _weightsCalculationTrigger?.Dispose();
        }
    }
}
