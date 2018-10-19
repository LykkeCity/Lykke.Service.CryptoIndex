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
using Lykke.Service.CryptoIndex.Domain.Repositories;
using Lykke.Service.CryptoIndex.Domain.Repositories.LCI10;

namespace Lykke.Service.CryptoIndex.Domain.Services.LCI10
{
    /// <summary>
    /// See the specification - https://lykkex.atlassian.net/secure/attachment/46308/LCI_specs.pdf
    /// </summary>
    public class LCI10Calculator : ILCI10Calculator, IStartable, IStopable
    {
        private const string Lci10 = "lci10";
        private readonly object _sync = new object();
        private bool _restarted;
        private readonly List<AssetMarketCap> _marketCaps;
        private readonly IDictionary<string, decimal> _weights;
        private readonly IIndexStateRepository _indexStateRepository;
        private readonly IIndexHistoryRepository _indexHistoryRepository;
        private readonly ISettingsService _settingsService;
        private readonly ITickPricesService _tickPricesService;
        private readonly ITickPricePublisher _tickPricePublisher;
        private readonly IMarketCapitalizationService _marketCapitalizationService;
        private readonly TimerTrigger _weightsCalculationTrigger;
        private readonly TimerTrigger _indexCalculationTrigger;
        private readonly ILog _log;

        public LCI10Calculator(IIndexStateRepository indexStateRepository, IIndexHistoryRepository indexHistoryRepository,
            ISettingsService settingsService, ITickPricePublisher tickPricePublisher, IMarketCapitalizationService marketCapitalizationService,
            TimeSpan weightsCalculationInterval, TimeSpan indexCalculationInterval, ITickPricesService tickPricesService, ILogFactory logFactory)
        {
            _marketCaps = new List<AssetMarketCap>();
            _weights = new ConcurrentDictionary<string, decimal>();

            _indexStateRepository = indexStateRepository ?? throw new ArgumentNullException(nameof(indexStateRepository));
            _indexHistoryRepository = indexHistoryRepository;
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _marketCapitalizationService = marketCapitalizationService ?? throw new ArgumentNullException(nameof(marketCapitalizationService));
            _tickPricePublisher = tickPricePublisher ?? throw new ArgumentNullException(nameof(tickPricePublisher));
            _tickPricesService = tickPricesService;

            _weightsCalculationTrigger = new TimerTrigger(nameof(LCI10Calculator), weightsCalculationInterval, logFactory, CalculateWeights);
            _indexCalculationTrigger = new TimerTrigger(nameof(LCI10Calculator), indexCalculationInterval, logFactory, CalculateIndex);

            _log = logFactory.CreateLog(this);
        }

        public async Task<IReadOnlyDictionary<string, decimal>> GetAssetMarketCapAsync()
        {
            var settings = await _settingsService.GetAsync();

            var result = new Dictionary<string, decimal>();

            foreach (var asset in settings.Assets)
            {
                var marketCap = _marketCaps.FirstOrDefault(x => x.Asset == asset);

                if (marketCap != null)
                    result.Add(asset, marketCap.MarketCap.Value);
            }

            return result;
        }

        public Task Reset()
        {
            lock (_sync)
            {
                _restarted = true;
                _indexStateRepository.Clear().GetAwaiter().GetResult();
                _indexHistoryRepository.Clear().GetAwaiter().GetResult();
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

            var settings = await _settingsService.GetAsync();

            var assets = settings.Assets.ToList();
            var allMarketCaps = await _marketCapitalizationService.GetAllAsync();
            var wantedMarketCaps = allMarketCaps.Where(x => assets.Contains(x.Asset)).ToArray();

            var notFoundAssets = assets.Where(x => !wantedMarketCaps.Select(mc => mc.Asset).Contains(x)).ToArray();
            if (wantedMarketCaps.Count() != assets.Count)
                throw new InvalidOperationException($"Can't find assets from settings in CoinMarketCap data: {notFoundAssets.ToJson()}.");

            var totalMarketCap = wantedMarketCaps.Select(x => x.MarketCap.Value).Sum();

            lock (_sync)
            {
                // Refresh market caps
                _marketCaps.Clear();
                _marketCaps.AddRange(wantedMarketCaps);

                // Calculate weights
                _weights.Clear();
                foreach (var asset in assets)
                {
                    var assetMarketCap = wantedMarketCaps.Single(x => x.Asset == asset).MarketCap.Value;
                    var assetWeight = assetMarketCap / totalMarketCap;
                    _weights[asset] = assetWeight;
                }
            }

            _log.Info($"Finished calculating weights for {assets.Count} assets.");
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
                _log.Warning($"Somethings went wrong while index calculation.", e);
            }
        }

        private async Task CalculateIndex()
        {
            _log.Info("Started calculating index...");

            var settings = await _settingsService.GetAsync();

            List<AssetMarketCap> marketCaps;
            IDictionary<string, decimal> weights;
            var assetsPrices = await _tickPricesService.GetPricesAsync();
            var lastIndex = await _indexStateRepository.GetAsync();

            RecalculateTheWeightsIfSomeWeightsAreNotFound(settings.Assets);

            lock (_sync)
            {
                marketCaps = _marketCaps.ToList();
                weights = _weights.Clone();
            }

            if (!IsAllDataPresent(marketCaps, weights, assetsPrices))
            {
                _log.Info("Skipped LCI10 calculation.");
                return;
            }

            lock (_sync)
            {
                if (_restarted || !Settings.Enabled)
                {
                    _restarted = false;
                    return;
                }       
            }

            // Save index as previous for the next execution
            var indexState = CalculateIndexState(settings.Assets, weights, assetsPrices, lastIndex);
            await _indexStateRepository.SetAsync(indexState);

            // Save index to history
            var indexHistory = new IndexHistory(indexState.Value, marketCaps, weights, assetsPrices, indexState.MiddlePrices, DateTime.UtcNow);
            await _indexHistoryRepository.InsertAsync(indexHistory);

            // Publish index to RabbitMq
            var tickPrice = new Models.TickPrice(Lci10, Lci10, indexHistory.Value, indexHistory.Value, indexHistory.Time);
            _tickPricePublisher.Publish(tickPrice);

            _log.Info($"Finished calculating index for {settings.Assets.Count} assets, value: {indexState.Value}.");
        }

        private static IndexState CalculateIndexState(IReadOnlyCollection<string> assets, IDictionary<string, decimal> assetsWeights,
            IDictionary<string, IDictionary<string, decimal>> assetsPrices, IndexState lastIndex)
        {
            var middlePrices = GetMiddlePrices(assetsPrices);

            if (lastIndex == null)
                lastIndex = new IndexState(1000, middlePrices);

            CheckThatAllAssetsArePresent(assets, assetsWeights, assetsPrices);

            var rLci10 = 0m;

            foreach (var asset in assets)
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

        private static void CheckThatAllAssetsArePresent(IReadOnlyCollection<string> assets, IDictionary<string, decimal> assetsWeights,
            IDictionary<string, IDictionary<string, decimal>> assetsPrices)
        {
            if (assetsWeights.Count != assets.Count || assetsPrices.Count != assets.Count)
                throw new InvalidOperationException("Some assets are missed, " +
                                                    $"settings: {assets.ToJson()}, " +
                                                    $"assetsWeights: {assetsWeights.Keys.ToJson()}, " +
                                                    $"assetsPrices: {assetsPrices.Keys.ToJson()}.");
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

        private void RecalculateTheWeightsIfSomeWeightsAreNotFound(IEnumerable<string> assets)
        {
            IDictionary<string, decimal> currentWeights;
            lock (_sync)
            {
                currentWeights = _weights.Clone();
            }

            if (assets.Any(x => !currentWeights.Keys.Contains(x)))
                CalculateWeights().GetAwaiter().GetResult();
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

        private Settings Settings => _settingsService.GetAsync().GetAwaiter().GetResult();

        private void SetEnabled(bool enabled)
        {
            var settings = new Settings(Settings.Sources, Settings.Assets, enabled);
            _settingsService.SetAsync(settings).GetAwaiter().GetResult();
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
