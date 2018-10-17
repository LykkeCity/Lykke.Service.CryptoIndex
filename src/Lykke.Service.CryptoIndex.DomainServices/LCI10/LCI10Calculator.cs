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
using Lykke.Service.CryptoIndex.Domain;
using Lykke.Service.CryptoIndex.Domain.LCI10;
using Lykke.Service.CryptoIndex.Domain.LCI10.IndexHistory;
using Lykke.Service.CryptoIndex.Domain.LCI10.IndexState;
using Lykke.Service.CryptoIndex.Domain.LCI10.Settings;
using Lykke.Service.CryptoIndex.Domain.MarketCapitalization;
using Lykke.Service.CryptoIndex.Domain.TickPrice;

namespace Lykke.Service.CryptoIndex.DomainServices.LCI10
{
    /// <summary>
    /// See the specification - https://lykkex.atlassian.net/secure/attachment/46308/LCI_specs.pdf
    /// </summary>
    public class LCI10Calculator : ILCI10Calculator, ITickPriceHandler, IStartable, IStopable
    {
        private const string Lci10 = "lci10";
        private const string Usd = "USD";
        private readonly object _sync = new object();
        private readonly List<AssetMarketCap> _marketCaps;
        private readonly IDictionary<string, decimal> _weights;
        private readonly IDictionary<string, IDictionary<string, decimal>> _pricesCache;
        private readonly IIndexStateRepository _indexStateRepository;
        private readonly IIndexHistoryRepository _indexHistoryRepository;
        private readonly ISettingsService _settingsService;
        private readonly IMarketCapitalizationService _marketCapitalizationService;
        private readonly ITickPricePublisher _tickPricePublisher;
        private readonly TimerTrigger _weightsCalculationTrigger;
        private readonly TimerTrigger _indexCalculationTrigger;
        private readonly ILog _log;

        public LCI10Calculator(IIndexStateRepository indexStateRepository, IIndexHistoryRepository indexHistoryRepository,
            ISettingsService settingsService, IMarketCapitalizationService marketCapitalizationService, ITickPricePublisher tickPricePublisher,
            TimeSpan weightsCalculationInterval, TimeSpan indexCalculationInterval, ILogFactory logFactory)
        {
            _marketCaps = new List<AssetMarketCap>();
            _weights = new ConcurrentDictionary<string, decimal>();
            _pricesCache = new ConcurrentDictionary<string, IDictionary<string, decimal>>();

            _indexStateRepository = indexStateRepository ?? throw new ArgumentNullException(nameof(indexStateRepository));
            _indexHistoryRepository = indexHistoryRepository;
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _marketCapitalizationService = marketCapitalizationService ?? throw new ArgumentNullException(nameof(marketCapitalizationService));
            _tickPricePublisher = tickPricePublisher ?? throw new ArgumentNullException(nameof(tickPricePublisher));

            _weightsCalculationTrigger = new TimerTrigger(nameof(LCI10Calculator), weightsCalculationInterval, logFactory, CalculateWeights);
            _indexCalculationTrigger = new TimerTrigger(nameof(LCI10Calculator), indexCalculationInterval, logFactory, CalculateIndex);

            _log = logFactory.CreateLog(this);
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

        public async Task HandleAsync(TickPrice tickPrice)
        {
            if (!tickPrice.AssetPair.ToUpper().EndsWith(Usd) || !tickPrice.Ask.HasValue)
                return;

            var asset = tickPrice.AssetPair.ToUpper().Replace(Usd, "");

            var settings = await _settingsService.GetAsync();

            if (!settings.Assets.Contains(asset))
                return;

            if (!settings.Sources.Contains(tickPrice.Source))
                return;

            if (!_pricesCache.ContainsKey(asset))
            {
                var newDictionary = new ConcurrentDictionary<string, decimal>
                {
                    [tickPrice.Source] = tickPrice.Ask.Value
                };
                _pricesCache[asset] = newDictionary;
            }
            else
            {
                var exchangesPrices = _pricesCache[asset];
                exchangesPrices[tickPrice.Source] = tickPrice.Ask.Value;
            }
        }

        public async Task<IDictionary<string, decimal>> GetAssetPricesAsync(string asset)
        {
            if (!_pricesCache.ContainsKey(asset))
                return new Dictionary<string, decimal>();

            return _pricesCache[asset];
        }

        public async Task<decimal?> GetAssetMarketCapAsync(string asset)
        {
            return _marketCaps.FirstOrDefault(x => x.Asset == asset)?.MarketCap.Value;
        }

        private async Task CalculateWeights(ITimerTrigger timer, TimerTriggeredHandlerArgs args, CancellationToken ct)
        {
            try
            {
                await CalculateWeights();
            }
            catch (Exception e)
            {
                _log.Error(e);
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
                _marketCaps.Clear();
                _marketCaps.AddRange(wantedMarketCaps);
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
                await CalculateIndex();
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
        }

        private async Task CalculateIndex()
        {
            _log.Info("Started calculating index...");

            var settings = await _settingsService.GetAsync();

            List<AssetMarketCap> marketCaps;
            IDictionary<string, decimal> weights;
            IDictionary<string, IDictionary<string, decimal>> assetsPrices;
            var lastIndex = await _indexStateRepository.GetAsync();

            if (!IsAllDataArePresented())
            {
                _log.Info("Skipped LCI10 calculation.");
                return;
            }

            RecalculateTheWeightsIfSomeWeightsAreNotFound(settings.Assets);

            lock (_sync)
            {
                marketCaps = _marketCaps.ToList();
                weights = _weights.Clone();
                assetsPrices = _pricesCache.Clone();
            }

            // Save index as previous for the next execution
            var indexState = CalculateIndexState(settings.Assets, weights, assetsPrices, lastIndex);
            await _indexStateRepository.SetAsync(indexState);

            // Save index to history
            var indexHistory = new IndexHistory(indexState.Value, marketCaps, weights, indexState.MiddlePrices, DateTime.UtcNow);
            await _indexHistoryRepository.InsertAsync(indexHistory);

            // Publish index to RabbitMq
            var tickPrice = new TickPrice(Lci10, Lci10, indexHistory.Value, indexHistory.Value, indexHistory.Time);
            _tickPricePublisher.Publish(tickPrice);

            _log.Info($"Finished calculating index for {settings.Assets.Count} assets, value: {indexState.Value}.");
        }

        private IndexState CalculateIndexState(IReadOnlyCollection<string> assets, IDictionary<string, decimal> assetsWeights,
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

        private static decimal GetPreviousMiddlePrice(string asset, IDictionary<string, decimal> previousPrices, decimal currentMiddlePrice)
        {
            return previousPrices.ContainsKey(asset)  // previous prices found in DB?
                ? previousPrices[asset]               // yes, use them
                : currentMiddlePrice;                 // no, use current
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

        private void RecalculateTheWeightsIfSomeWeightsAreNotFound(IReadOnlyList<string> assets)
        {
            IDictionary<string, decimal> currentWeights;
            lock (_sync)
            {
                currentWeights = _weights.Clone();
            }

            if (assets.Any(x => !currentWeights.Keys.Contains(x)))
                CalculateWeights().GetAwaiter().GetResult();
        }

        private bool IsAllDataArePresented()
        {
            if (!_marketCaps.Any())
            {
                _log.Info("Market Cap data is not filled yet.");
                return false;
            }

            if (!_weights.Any())
            {
                _log.Info("Assets Weights is not calculated yet.");
                return false;
            }

            if (!_pricesCache.Any())
            {
                _log.Info("Assets Prices cache is not filled yet.");
                return false;
            }

            return true;
        }
    }
}
