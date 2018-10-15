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
using Lykke.Service.CryptoIndex.Domain.LCI10.IndexSnapshot;
using Lykke.Service.CryptoIndex.Domain.LCI10.Settings;
using Lykke.Service.CryptoIndex.Domain.MarketCapitalization;
using Lykke.Service.CryptoIndex.Domain.TickPrice;
using MoreLinq;

namespace Lykke.Service.CryptoIndex.DomainServices.LCI10
{
    /// <summary>
    /// See the specification - https://lykkex.atlassian.net/secure/attachment/46308/LCI_specs.pdf
    /// </summary>
    public class LCI10Calculator : ILCI10Calculator, ITickPriceHandler, IStartable, IStopable
    {
        private const string Usd = "USD";
        private readonly object _sync = new object();
        private readonly List<AssetMarketCap> _marketCaps;
        private readonly IDictionary<string, decimal> _assetsWeights;
        private readonly IDictionary<string, IDictionary<string, decimal>> _assetsPricesCache;
        private readonly IIndexSnapshotRepository _indexSnapshotRepository;
        private readonly ISettingsService _settingsService;
        private readonly IMarketCapitalizationService _marketCapitalizationService;
        private readonly TimerTrigger _weightsCalculationTrigger;
        private readonly TimerTrigger _indexCalculationTrigger;
        private readonly ILog _log;

        public LCI10Calculator(IIndexSnapshotRepository indexSnapshotRepository, ISettingsService settingsService,
            IMarketCapitalizationService marketCapitalizationService,
            TimeSpan weightsCalculationInterval, TimeSpan indexCalculationInterval, ILogFactory logFactory)
        {
            _marketCaps = new List<AssetMarketCap>();
            _assetsWeights = new ConcurrentDictionary<string, decimal>();
            _assetsPricesCache = new ConcurrentDictionary<string, IDictionary<string, decimal>>();

            _indexSnapshotRepository = indexSnapshotRepository ?? throw new ArgumentNullException(nameof(indexSnapshotRepository));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _marketCapitalizationService = marketCapitalizationService ?? throw new ArgumentNullException(nameof(marketCapitalizationService));

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

            if (!_assetsPricesCache.ContainsKey(asset))
            {
                var newDictionary = new ConcurrentDictionary<string, decimal>
                {
                    [tickPrice.Source] = tickPrice.Ask.Value
                };
                _assetsPricesCache[asset] = newDictionary;
            }
            else
            {
                var exchangesPrices = _assetsPricesCache[asset];
                exchangesPrices[tickPrice.Source] = tickPrice.Ask.Value;
            }
        }

        public async Task<IDictionary<string, decimal>> GetAssetPricesAsync(string asset)
        {
            if (!_assetsPricesCache.ContainsKey(asset))
                return new Dictionary<string, decimal>();

            return _assetsPricesCache[asset];
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
                _assetsWeights.Clear();
                foreach (var asset in assets)
                {
                    var assetMarketCap = wantedMarketCaps.Single(x => x.Asset == asset).MarketCap.Value;
                    var assetWeight = assetMarketCap / totalMarketCap;
                    _assetsWeights[asset] = assetWeight;
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

            IList<AssetMarketCap> marketCaps;
            IDictionary<string, decimal> assetsWeights;
            IDictionary<string, IDictionary<string, decimal>> assetsPrices;
            var previousIndex = await _indexSnapshotRepository.GetLatestAsync();

            if (!IsAllDataArePresented())
            {
                _log.Info("Skipped LCI10 calculation.");
                return;
            }

            RecalculateTheWeightsIfSomeWeightIsNotFound(settings);

            lock (_sync)
            {
                marketCaps = _marketCaps.ToList();
                assetsWeights = _assetsWeights.Clone();
                assetsPrices = _assetsPricesCache.Clone();
            }

            if (previousIndex == null)
                previousIndex = new IndexSnapshot(1000, marketCaps, assetsWeights, assetsPrices, DateTimeOffset.UtcNow);

            CheckThatAllAssetsArePresent(settings, assetsWeights, assetsPrices);

            var signalLci10 = 0m;
            foreach (var asset in settings.Assets)
            {
                var middlePrice = GetMiddlePrice(asset, assetsPrices);
                var previousMiddlePrice = !previousIndex.Prices.ContainsKey(asset)
                    ? GetMiddlePrice(asset, assetsPrices)
                    : middlePrice;

                if (!previousIndex.Prices.ContainsKey(asset))
                    _log.Info($"Has not found previous prices for '{asset}', got the current ones.");

                var weight = assetsWeights[asset];
                var signal = middlePrice / previousMiddlePrice; // signal is 'R' in specification

                signalLci10 += weight * signal;
            }

            var index = previousIndex.Value * signalLci10;

            var indexSnapshot = new IndexSnapshot(index, marketCaps, assetsWeights, assetsPrices, DateTimeOffset.UtcNow);

            await _indexSnapshotRepository.InsertAsync(indexSnapshot);

            _log.Info($"Finished calculating index for {settings.Assets.Count} assets.");
        }

        private static void CheckThatAllAssetsArePresent(Settings settings, IDictionary<string, decimal> assetsWeights,
            IDictionary<string, IDictionary<string, decimal>> assetsPrices)
        {
            if (assetsWeights.Count != settings.Assets.Count || assetsPrices.Count != settings.Assets.Count)
                throw new InvalidOperationException("Some assets are missed, " +
                                                    $"settings: {settings.Assets.ToJson()}, " +
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

        private void RecalculateTheWeightsIfSomeWeightIsNotFound(Settings settings)
        {
            IDictionary<string, decimal> currentWeights;
            lock (_sync)
            {
                currentWeights = _assetsWeights.Clone();
            }

            if (settings.Assets.Any(x => !currentWeights.Keys.Contains(x)))
                CalculateWeights().GetAwaiter().GetResult();
        }

        private bool IsAllDataArePresented()
        {
            if (!_marketCaps.Any())
            {
                _log.Info("Market Cap data is not filled yet.");
                return false;
            }

            if (!_assetsWeights.Any())
            {
                _log.Info("Assets Weights is not calculated yet.");
                return false;
            }

            if (!_assetsPricesCache.Any())
            {
                _log.Info("Assets Prices cache is not filled yet.");
                return false;
            }

            return true;
        }
    }
}
