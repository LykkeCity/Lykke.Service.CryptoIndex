using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.CryptoIndex.Domain.Models;
using Lykke.Service.CryptoIndex.Domain.Publishers;
using Lykke.Service.CryptoIndex.Domain.Repositories;
using MoreLinq;

namespace Lykke.Service.CryptoIndex.Domain.Services
{
    /// <summary>
    /// See the specification - https://lykkex.atlassian.net/secure/attachment/46308/LCI_specs.pdf
    /// </summary>
    public class IndexCalculator : IIndexCalculator, IStartable, IStopable
    {
        private const string RabbitMqSource = "LCI";
        private const decimal InitialIndexValue = 1000m;
        private readonly TimeSpan _waitForTopAssetsPricesFromStart = TimeSpan.FromMinutes(2);
        private readonly string _indexName;
        private readonly DateTime _startedAt;

        private readonly object _sync = new object();
        private readonly List<AssetMarketCap> _allMarketCaps;
        private readonly List<string> _topAssets;
        private IReadOnlyCollection<string> TopAssets { get { lock (_sync) { return _topAssets.ToList(); } } }

        private DateTime _lastRebuild;
        private bool _isRebuild;

        private readonly TimerTrigger _trigger;
        private readonly ILog _log;

        private readonly ISettingsService _settingsService;
        private readonly IIndexStateRepository _indexStateRepository;
        private readonly IIndexHistoryRepository _indexHistoryRepository;
        private readonly ITickPricesService _tickPricesService;
        private readonly ICoinMarketCapService _coinMarketCapService;
        private readonly ITickPricePublisher _tickPricePublisher;
        private readonly IWarningRepository _warningRepository;
        private readonly IFirstStateAfterResetTimeRepository _firstStateAfterResetTimeRepository;

        private Settings Settings => _settingsService.GetAsync().GetAwaiter().GetResult();
        private IndexState State => _indexStateRepository.GetAsync().GetAwaiter().GetResult();

        public IndexCalculator(string indexName, 
            TimeSpan indexCalculationInterval,
            ISettingsService settingsService,
            IIndexStateRepository indexStateRepository,
            IIndexHistoryRepository indexHistoryRepository,
            ITickPricesService tickPricesService,
            ICoinMarketCapService coinMarketCapService,
            ITickPricePublisher tickPricePublisher,
            IWarningRepository warningRepository,
            IFirstStateAfterResetTimeRepository firstStateAfterResetTimeRepository,
            ILogFactory logFactory)
        {
            _startedAt = DateTime.UtcNow;
            _lastRebuild = DateTime.UtcNow.Date;
            _allMarketCaps = new List<AssetMarketCap>();
            _topAssets = new List<string>();

            _indexName = indexName;
            _trigger = new TimerTrigger(nameof(IndexCalculator), indexCalculationInterval, logFactory, TimerHandler);

            _settingsService = settingsService;
            _indexStateRepository = indexStateRepository;
            _indexHistoryRepository = indexHistoryRepository;
            _tickPricesService = tickPricesService;
            _coinMarketCapService = coinMarketCapService;
            _tickPricePublisher = tickPricePublisher;
            _warningRepository = warningRepository;
            _firstStateAfterResetTimeRepository = firstStateAfterResetTimeRepository;

            _log = logFactory.CreateLog(this);
        }

        public void Start()
        {
            Initialize(); // top assets and weights from the last history state

            _trigger.Start();
        }

        public IReadOnlyDictionary<string, decimal> GetAllAssetsMarketCaps()
        {
            var result = new Dictionary<string, decimal>();

            lock (_sync)
            {
                foreach (var x in _allMarketCaps)
                    result.Add(x.Asset, x.MarketCap.Value);
            }

            return result;
        }

        public async Task Reset()
        {
            await _indexStateRepository.Clear();

            lock (_sync)
            {
                _isRebuild = true;
            }
        }

        public async Task Rebuild()
        {
            await RefreshCoinMarketCapDataAsync();

            lock (_sync)
            {
                _isRebuild = true;
            }
        }
        

        private void Initialize()
        {
            _log.Info("Initializing last state from history if needed...");

            try
            {
                // Initialize _allMarketCaps
                RefreshCoinMarketCapDataAsync().GetAwaiter().GetResult();

                lock (_sync)
                {
                    // Get latest index history element
                    var lastIndexHistory = _indexHistoryRepository.TakeLastAsync(1).GetAwaiter().GetResult().SingleOrDefault();
                    if (lastIndexHistory == null)
                    {
                        if (Settings.Assets.Any())
                            RebuildTopAssetsAsync().GetAwaiter().GetResult();

                        _log.Info("Skipping initializing previous state, last index history is empty.");
                        return;
                    }

                    // Initialize _topAssets
                    _topAssets.AddRange(lastIndexHistory.Weights.Keys);

                    _log.Info("Initialized previous weights and market caps from history.");
                }
            }
            catch (Exception e)
            {
                _log.Warning("Can't initialize last state from history.", e);
            }
        }

        private async Task RefreshCoinMarketCapDataAsync()
        {
            _log.Info("Requesting CoinMarketCap data....");

            IReadOnlyList<AssetMarketCap> allMarketCaps;
            try
            {
                // Get top 100 market caps
                allMarketCaps = await _coinMarketCapService.GetAllAsync();
            }
            catch (Exception e)
            {
                _log.Warning("Can't request data from CoinMarketCap.", e);
                return;
            }

            lock (_sync)
            {
                // Refresh market caps
                _allMarketCaps.Clear();
                _allMarketCaps.AddRange(allMarketCaps);
            }

            _log.Info("Finished requesting CoinMarketCap data....");
        }

        private async Task RebuildTopAssetsAsync()
        {
            _log.Info("Recalculating top asset....");

            if (!_allMarketCaps.Any())
            {
                _log.Warning("Coin Market Cap data is empty while calculating top assets. Skipped.");
                return;
            }

            var settings = Settings;

            // Get top 100 market caps
            List<AssetMarketCap> coinMarketCapData;
            lock (_sync)
            {
                coinMarketCapData = _allMarketCaps.ToList();
            }

            // Get white list supplies
            var whiteListSupplies = new Dictionary<string, decimal>();
            settings.Assets.ForEach(x => whiteListSupplies.Add(x, coinMarketCapData.Single(mk => mk.Asset == x).CirculatingSupply));

            // Get white list prices
            var sources = settings.Sources.ToList();
            var whiteListAssets = whiteListSupplies.Select(x => x.Key).ToList();
            var allAssetsPrices = await _tickPricesService.GetPricesAsync(sources);
            var assetsSettings = settings.AssetsSettings;
            var whiteListUsingPrices = GetAssetsUsingPrices(whiteListAssets, allAssetsPrices, assetsSettings);

            if (!IsAllPricesArePresent(whiteListAssets, whiteListUsingPrices))
            {
                _log.Info($"Skipped calculating top assets because some prices are not present yet, waiting for them for {_waitForTopAssetsPricesFromStart.TotalMinutes} minutes since start.");
                return;
            }

            // Calculate white list market caps
            var whiteListMarketCaps = CalculateMarketCaps(whiteListAssets, whiteListSupplies, whiteListUsingPrices);

            // Calculate white list weights
            var whiteListWeights = CalculateWeightsOrderedByDesc(whiteListMarketCaps);

            // Set top N assets weights
            var topWeights = whiteListWeights
                .Take(Settings.TopCount)
                .ToDictionary();

            lock (_sync)
            {
                // Refresh weights
                _topAssets.Clear();
                _topAssets.AddRange(topWeights.Keys);

                _lastRebuild = DateTime.UtcNow.Date;

                _isRebuild = false;
            }

            _log.Info($"Finished calculating top assets, count - {_topAssets.Count}.");
        }

        private async Task TimerHandler(ITimerTrigger timer, TimerTriggeredHandlerArgs args, CancellationToken ct)
        {
            try
            {
                var rebuildNeeded = _isRebuild
                                    || (_lastRebuild.Date < DateTime.UtcNow.Date // last rebuild was yesterday
                                        && DateTime.UtcNow.TimeOfDay > Settings.RebuildTime); // now > rebuild time
                if (rebuildNeeded)
                    await RebuildTopAssetsAsync();

                await CalculateThenSaveAndPublish();
            }
            catch (Exception e)
            {
                _log.Warning("Somethings went wrong in timer handler.", e);
            }
        }

        private async Task CalculateThenSaveAndPublish()
        {
            _log.Info("Started calculating index...");

            var settings = Settings;
            var whiteListAssets = settings.Assets;
            if (!whiteListAssets.Any())
            {
                _log.Info("There are no assets in the white list, skipped index calculation.");
                return;
            }

            var topAssets = TopAssets; // Must be obtained from _topAssets (daily rebuild changes it)
            if (!topAssets.Any())
            {
                _log.Info("There are no top assets yet, skipped index calculation.");
                return;
            }

            var lastIndex = await _indexStateRepository.GetAsync();
            var sources = settings.Sources.ToList();
            var allPrices = await _tickPricesService.GetPricesAsync(sources);
            var assetsSettings = settings.AssetsSettings;
            var topUsingPrices = GetAssetsUsingPrices(topAssets, allPrices, assetsSettings);

            // If just started and prices are not present yet, then skip.
            // If started more then {_waitForTopAssetsPricesFromStart} ago then write warning to DB and log.
            if (!IsAllPricesArePresent(topAssets, topUsingPrices))
            {
                _log.Info($"Skipped calculating index because some prices are not present yet, waiting for them for {_waitForTopAssetsPricesFromStart.TotalMinutes} minutes since start.");
                return;
            }

            CheckAutoFreeze(topAssets, topUsingPrices, lastIndex, settings);
            assetsSettings = Settings.AssetsSettings;
            topUsingPrices = GetAssetsUsingPrices(topAssets, allPrices, assetsSettings);

            // Recalculate top market caps with supplies and current using prices for previous index assets
            var topSupplies = new Dictionary<string, decimal>();
            _allMarketCaps.Where(x => topAssets.Contains(x.Asset))
                .ForEach(x => topSupplies.Add(x.Asset, x.CirculatingSupply));

            var calculatedTopMarketCaps = CalculateMarketCaps(topAssets, topSupplies, topUsingPrices);

            var calculatedTopWeights = CalculateWeightsOrderedByDesc(calculatedTopMarketCaps);

            // Get current index state
            var indexState = CalculateIndex(lastIndex, calculatedTopWeights, topUsingPrices);

            var indexHistory = new IndexHistory(indexState.Value, calculatedTopMarketCaps, calculatedTopWeights, allPrices, topUsingPrices, DateTime.UtcNow, assetsSettings);

            // if there was a reset then skip until next iteration which will have initial state
            if (State == null)
            {
                if (indexState.Value != InitialIndexValue)
                {
                    _log.Info($"Skipped saving and publishing index because of reset - previous state is null and current index not equals {InitialIndexValue}.");
                    return;
                }
                
                await _firstStateAfterResetTimeRepository.SetAsync(indexHistory.Time);
                _log.Info($"Reset with time: {indexHistory.Time.ToIsoDateTime()}.");
            }

            await Save(indexState, indexHistory);

            Publish(indexHistory, assetsSettings);

            _log.Info($"Finished calculating index for {calculatedTopMarketCaps.Count} assets, value: {indexState.Value}.");
        }

        private IndexState CalculateIndex(IndexState lastIndex, IDictionary<string, decimal> topAssetsWeights,
            IDictionary<string, decimal> topUsingPrices)
        {
            if (lastIndex == null)
            {
                return new IndexState(InitialIndexValue, topUsingPrices);
            }

            var signal = 0m;

            var topAssets = topAssetsWeights.Keys.ToList();
            foreach (var asset in topAssets)
            {
                var currentPrice = topUsingPrices[asset];
                var previousPrice = Utils.GetPreviousMiddlePrice(asset, lastIndex, currentPrice);

                var weight = topAssetsWeights[asset];

                var r = currentPrice / previousPrice;

                signal += weight * r;
            }

            var indexValue = Math.Round(lastIndex.Value * signal, 2);

            ValidateIndexValue(indexValue, topAssetsWeights, topUsingPrices, lastIndex);

            var indexState = new IndexState(indexValue, topUsingPrices);

            return indexState;
        }

        private async Task Save(IndexState indexState, IndexHistory indexHistory)
        {
            // Skip if changed to 'disabled'
            if (!Settings.Enabled)
            {
                _log.Info($"Skipped saving index because {nameof(Settings)}.{nameof(Settings.Enabled)} = {Settings.Enabled}.");
                return;
            }

            // Save index state for the next execution
            await _indexStateRepository.SetAsync(indexState);

            // Save index history
            await _indexHistoryRepository.InsertAsync(indexHistory);
        }

        private void Publish(IndexHistory indexHistory, IReadOnlyList<AssetSettings> assetsSettings)
        {
            // Skip if changed to 'disabled'
            if (!Settings.Enabled)
            {
                _log.Info($"Skipped publishing index because {nameof(Settings)}.{nameof(Settings.Enabled)} = {Settings.Enabled}.");
                return;
            }

            var assetsInfo = new List<AssetInfo>();
            var frozenAssets = assetsSettings.Where(x => x.IsDisabled).Select(x => x.AssetId).ToList();
            foreach (var asset in indexHistory.Weights.Keys.ToList())
            {
                var isFrozen = frozenAssets.Contains(asset);
                assetsInfo.Add(new AssetInfo(asset, indexHistory.Weights[asset], indexHistory.MiddlePrices[asset], isFrozen));
            }

            // Publish index to RabbitMq
            var tickPrice = new IndexTickPrice(RabbitMqSource, _indexName.ToUpper(), indexHistory.Value, indexHistory.Value, indexHistory.Time, assetsInfo);
            _tickPricePublisher.Publish(tickPrice);
        }

        private IReadOnlyList<AssetMarketCap> CalculateMarketCaps(IReadOnlyCollection<string> assets,
            IDictionary<string, decimal> supplies, IDictionary<string, decimal> usingPrices)
        {
            if (assets.Count != supplies.Count || assets.Count != usingPrices.Count)
            {
                throw new InvalidOperationException("Can't calculate weights, some data are missed." +
                                                    $"Assets: {assets.ToJson()}." +
                                                    $"Supplies: {supplies.Select(x => x.Key).ToList().ToJson()}." +
                                                    $"Prices: {usingPrices.Select(x => x.Key).ToList().ToJson()}.");
            }

            var marketCaps = new Dictionary<string, decimal>();

            foreach (var asset in assets)
            {
                var supply = supplies[asset];
                var price = usingPrices[asset];
                marketCaps.Add(asset, price * supply);
            }

            var result = new List<AssetMarketCap>();

            foreach (var marketCap in marketCaps)
                result.Add(new AssetMarketCap(marketCap.Key, new MarketCap(marketCap.Value, "USD"), supplies[marketCap.Key]));

            return result;
        }

        private IDictionary<string, decimal> CalculateWeightsOrderedByDesc(IReadOnlyList<AssetMarketCap> marketCaps)
        {
            var weights = new List<(string Asset, decimal Weight)>();

            // Sum of top market caps
            var totalMarketCap = marketCaps.Select(x => x.MarketCap.Value).Sum();

            // Calculate weights
            foreach (var marketCap in marketCaps)
            {
                var assetWeight = marketCap.MarketCap.Value / totalMarketCap;
                weights.Add((marketCap.Asset, assetWeight));
            }

            weights = weights.OrderByDescending(x => x.Weight).ToList();

            var result = new Dictionary<string, decimal>();
            weights.ForEach(x => result.Add(x.Asset, x.Weight));

            return result;
        }

        private IDictionary<string, decimal> GetAssetsUsingPrices(IReadOnlyCollection<string> assets,
            IDictionary<string, IDictionary<string, decimal>> allPrices, IReadOnlyCollection<AssetSettings> assetsSettings)
        {
            var topAssetsUsedPrices = new Dictionary<string, decimal>();

            foreach (var asset in assets.ToList())
            {
                if (!allPrices.ContainsKey(asset) || allPrices[asset].Count == 0)
                    continue;

                var assetPrices = allPrices[asset];

                var currentMiddlePrice = Utils.GetMiddlePrice(asset, assetPrices.Values.ToList());

                var assetSettings = assetsSettings.FirstOrDefault(x => x.AssetId == asset);

                if (assetSettings != null && assetSettings.IsDisabled)
                    currentMiddlePrice = assetSettings.Price;

                topAssetsUsedPrices[asset] = currentMiddlePrice;
            }

            return topAssetsUsedPrices;
        }

        private void CheckAutoFreeze(IReadOnlyCollection<string> topAssets,
            IDictionary<string, decimal> whiteListAssetsMiddlePrices,
            IndexState lastIndex,
            Settings settings)
        {
            if (lastIndex == null || settings.AutoFreezeChangePercents == default(decimal))
                return;

            var assetsSettings = settings.AssetsSettings.ToList();

            foreach (var asset in topAssets)
            {
                var assetSettings = assetsSettings.SingleOrDefault(x => x.AssetId == asset);

                if (assetSettings != null && assetSettings.IsDisabled)
                    continue;

                var middlePrice = whiteListAssetsMiddlePrices[asset];
                var previousMiddlePrice = Utils.GetPreviousMiddlePrice(asset, lastIndex, middlePrice);

                var changePercents = Math.Abs((previousMiddlePrice - middlePrice) / previousMiddlePrice * 100);

                if (changePercents >= settings.AutoFreezeChangePercents)
                {
                    if (assetSettings != null)
                        assetsSettings.Remove(assetsSettings.Single(x => x.AssetId == asset));

                    assetSettings = new AssetSettings(asset, previousMiddlePrice, true, true);
                    assetsSettings.Add(assetSettings);

                    settings.AssetsSettings = assetsSettings;
                    _settingsService.SetAsync(settings).GetAwaiter().GetResult();
                }
            }
        }

        private bool IsAllPricesArePresent(IReadOnlyCollection<string> assets, IDictionary<string, decimal> assetsUsingPrices)
        {
            if (!assets.Any())
                return false;

            if (!assetsUsingPrices.Any())
                return false;

            var topAssetsWoPrices = new List<string>();
            foreach (var topAsset in assets)
            {
                if (!assetsUsingPrices.ContainsKey(topAsset))
                    topAssetsWoPrices.Add(topAsset);
            }

            if (topAssetsWoPrices.Any())
            {
                var message = $"Some assets don't have prices: {topAssetsWoPrices.ToJson()}.";

                // If just started then skip current iteration
                if (DateTime.UtcNow - _startedAt > _waitForTopAssetsPricesFromStart)
                {
                    _warningRepository.SaveAsync(new Warning(message, DateTime.UtcNow));
                    throw new InvalidOperationException(message);
                }
                
                return false;
            }

            return true;
        }

        private void ValidateIndexValue(decimal indexValue, IDictionary<string, decimal> topAssetWeights,
            IDictionary<string, decimal> allAssetsMiddlePrices, IndexState lastIndex)
        {
            if (indexValue > 0)
                return;

            var message = $"Index value less or equals to 0: topAssetWeights = {topAssetWeights.ToJson()}, allAssetsPrices: {allAssetsMiddlePrices.ToJson()}, lastIndex: {lastIndex.ToJson()}.";
            _warningRepository.SaveAsync(new Warning(message, DateTime.UtcNow));
            throw new InvalidOperationException(message);
        }

        public void Stop()
        {
            _trigger.Stop();
        }

        public void Dispose()
        {
            _trigger?.Dispose();

            _coinMarketCapService?.Dispose();
        }
    }
}
