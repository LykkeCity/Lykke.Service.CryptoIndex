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

        private DateTime _lastRebuild;
        private bool _isRebuild;

        private readonly object _sync = new object();
        private readonly List<AssetMarketCap> _allMarketCaps;
        private readonly List<AssetMarketCap> _topMarketCaps; // TODO: посмотреть как расчитывается в каждом месте
        private readonly IDictionary<string, decimal> _topWeights;

        private readonly object _lastValueSync = new object();
        private decimal _lastValue;

        private readonly object _lastResetSync = new object();
        private DateTime? _lastReset;

        private readonly TimerTrigger _trigger;
        private readonly ILog _log;

        private IIndexStateRepository IndexStateRepository { get; set; }
        private IFirstStateAfterResetTimeRepository FirstStateAfterResetTimeRepository { get; set; }
        private IIndexHistoryRepository IndexHistoryRepository { get; set; }
        private IWarningRepository WarningRepository { get; set; }
        private ISettingsService SettingsService { get; set; }
        private ITickPricesService TickPricesService { get; set; }
        private ITickPricePublisher TickPricePublisher { get; set; }
        private ICoinMarketCapService CoinMarketCapService { get; set; }

        private Settings Settings => SettingsService.GetAsync().GetAwaiter().GetResult();
        private IndexState State => IndexStateRepository.GetAsync().GetAwaiter().GetResult();
        private IDictionary<string, decimal> TopWeights { get { lock (_sync) { return _topWeights.Clone(); } } }

        public IndexCalculator(string indexName, TimeSpan indexCalculationInterval, ILogFactory logFactory)
        {
            _startedAt = DateTime.UtcNow;
            _lastRebuild = DateTime.UtcNow.Date;
            _allMarketCaps = new List<AssetMarketCap>();
            _topMarketCaps = new List<AssetMarketCap>();
            _topWeights = new Dictionary<string, decimal>();

            _indexName = indexName;
            _trigger = new TimerTrigger(nameof(IndexCalculator), indexCalculationInterval, logFactory, TimerHandler);

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
            await IndexStateRepository.Clear();

            lock (_sync)
            {
                _isRebuild = true;
            }
        }

        public async Task Rebuild()
        {
            await RefreshCoinMarketCapData();

            lock (_sync)
            {
                _isRebuild = true;
            }
        }

        public decimal GetLastValue()
        {
            lock (_lastValueSync)
            {
                return _lastValue;
            }
        }

        public DateTime? GetLastResetTimestamp()
        {
            lock (_lastResetSync)
            {
                return _lastReset;
            }
        }


        private void Initialize()
        {
            _log.Info("Initializing last state from history if needed...");

            try
            {
                // Initialize _allMarketCaps
                RefreshCoinMarketCapData().GetAwaiter().GetResult();

                lock (_sync)
                {
                    // Get latest index history element
                    var lastIndexHistory = IndexHistoryRepository.TakeLastAsync(1).GetAwaiter().GetResult().SingleOrDefault();
                    if (lastIndexHistory == null)
                    {
                        _log.Info("Skipping initializing previous state, last index history is empty.");
                        return;
                    }

                    lock (_lastValueSync)
                        _lastValue = lastIndexHistory.Value;

                    lock (_lastResetSync)
                        _lastReset = FirstStateAfterResetTimeRepository.GetAsync().GetAwaiter().GetResult();

                    // Initialize _topMarketCaps and _topWeights

                    // Combined - weights are from previous index, supplies are from actual CoinMarketCap data.
                    var topMarketCapsCombined = new List<AssetMarketCap>();
                    foreach (var marketCap in lastIndexHistory.MarketCaps)
                    {
                        var circulatingSupply = _allMarketCaps.Single(x => x.Asset == marketCap.Asset).CirculatingSupply;
                        var combined = new AssetMarketCap(marketCap.Asset, marketCap.MarketCap, circulatingSupply);
                        topMarketCapsCombined.Add(combined);
                    }

                    _topMarketCaps.AddRange(topMarketCapsCombined);
                    lastIndexHistory.Weights.ForEach(x => _topWeights.Add(x.Key, x.Value));

                    _log.Info("Initialized previous weights and market caps from history.");
                }
            }
            catch (Exception e)
            {
                _log.Warning("Can't initialize last state from history.", e);
            }
        }

        private async Task RefreshCoinMarketCapData()
        {
            _log.Info("Requesting CoinMarketCap data....");

            IReadOnlyList<AssetMarketCap> allMarketCaps;
            try
            {
                // Get top 100 market caps
                allMarketCaps = await CoinMarketCapService.GetAllAsync();
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

        private async Task CalculateWeightsAndRebuild()
        {
            _log.Info("Calculating weights....");

            if (!_allMarketCaps.Any())
            {
                _log.Warning("Coin Market Cap data is empty while calculating weights.");
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
            Settings.Assets.ForEach(x => whiteListSupplies.Add(x, coinMarketCapData.Single(mk => mk.Asset == x).CirculatingSupply));

            // Get white list prices
            var sources = settings.Sources.ToList();
            var whiteListAssets = whiteListSupplies.Select(x => x.Key).ToList();
            var allAssetsPrices = await TickPricesService.GetPricesAsync(sources);
            var assetsSettings = settings.AssetsSettings;
            var whiteListUsingPrices = GetAssetsUsingPrices(whiteListAssets, allAssetsPrices, assetsSettings);

            if (!IsAllPricesArePresent(whiteListAssets, whiteListUsingPrices))
            {
                _log.Info($"Skipped calculating weights because some prices are not present yet, waiting for them for {_waitForTopAssetsPricesFromStart.TotalMinutes} minutes since start.");
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

            var topAssets = topWeights.Keys.ToList();

            var topMarketCaps = whiteListMarketCaps.Where(x => topAssets.Contains(x.Asset)).ToList();

            lock (_sync)
            {
                // Refresh market caps
                _topMarketCaps.Clear();
                _topMarketCaps.AddRange(topMarketCaps);

                // Refresh weights
                _topWeights.Clear();
                foreach (var assetWeight in topWeights)
                    _topWeights.Add(assetWeight);

                _lastRebuild = DateTime.UtcNow.Date;

                _isRebuild = false;
            }

            _log.Info($"Finished calculating weights, top assets weights count - {topWeights.Count}.");
        }

        private async Task TimerHandler(ITimerTrigger timer, TimerTriggeredHandlerArgs args, CancellationToken ct)
        {
            try
            {
                var rebuildNeeded = _isRebuild
                                    || (_lastRebuild.Date < DateTime.UtcNow.Date // last rebuild was yesterday
                                        && DateTime.UtcNow.TimeOfDay > Settings.RebuildTime); // now > rebuild time
                if (rebuildNeeded)
                {
                    await CalculateWeightsAndRebuild();

                    lock (_sync)
                    {
                        _isRebuild = false;
                    }
                }

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

            var topWeights = TopWeights;
            if (!topWeights.Any())
            {
                _log.Info("There are no weights for constituents yet, skipped index calculation.");
                return;
            }

            var sources = settings.Sources.ToList();
            var topAssets = topWeights.Keys.OrderBy(x => x).ToList(); // Must be obtained from _topWeights (daily rebuild changes it)
            var allPrices = await TickPricesService.GetPricesAsync(sources);
            var assetsSettings = settings.AssetsSettings;
            var topUsingPrices = GetAssetsUsingPrices(topAssets, allPrices, assetsSettings);

            // If just started and prices are not present yet, then skip.
            // If started more then {_waitForTopAssetsPricesFromStart} ago then write warning to DB and log.
            if (!IsAllPricesArePresent(topAssets, topUsingPrices))
            {
                _log.Info($"Skipped calculating index because some prices are not present yet, waiting for them for {_waitForTopAssetsPricesFromStart.TotalMinutes} minutes since start.");
                return;
            }

            // Recalculate top market caps with supplies and current using prices for previous index assets
            var topSupplies = new Dictionary<string, decimal>();
            _allMarketCaps.Where(x => topAssets.Contains(x.Asset))
                .ForEach(x => topSupplies.Add(x.Asset, x.CirculatingSupply));

            var calculatedTopMarketCaps = CalculateMarketCaps(topAssets, topSupplies, topUsingPrices);

            var calculatedTopWeights = CalculateWeightsOrderedByDesc(calculatedTopMarketCaps);

            // Get current index state
            var lastIndex = await IndexStateRepository.GetAsync();
            var indexState = await CalculateIndex(lastIndex, calculatedTopWeights, topUsingPrices);

            // if there was a reset then skip until next iteration which will have initial state
            if (indexState.Value != InitialIndexValue && State == null)
            {
                _log.Info($"Skipped saving and publishing index because of reset - previous state is null and current index not equals {InitialIndexValue}.");
                return;
            }
            
            var indexHistory = new IndexHistory(indexState.Value, calculatedTopMarketCaps, calculatedTopWeights, allPrices, topUsingPrices, DateTime.UtcNow, assetsSettings);

            await Save(indexState, indexHistory);

            lock (_lastValueSync)
            {
                _lastValue = indexHistory.Value;
            }

            Publish(indexHistory, assetsSettings);

            _log.Info($"Finished calculating index for {calculatedTopMarketCaps.Count} assets, value: {indexState.Value}.");
        }

        private async Task<IndexState> CalculateIndex(IndexState lastIndex, IDictionary<string, decimal> topAssetsWeights,
            IDictionary<string, decimal> whiteListAssetsMiddlePrices)
        {
            if (lastIndex == null)
            {
                await Rebuild(); // ???
                
                return new IndexState(InitialIndexValue, whiteListAssetsMiddlePrices);
            }

            var signal = 0m;

            var topAssets = topAssetsWeights.Keys.ToList();
            foreach (var asset in topAssets)
            {
                var middlePrice = whiteListAssetsMiddlePrices[asset];
                var previousMiddlePrice = GetPreviousMiddlePrice(asset, lastIndex, middlePrice);

                var weight = topAssetsWeights[asset];

                var r = middlePrice / previousMiddlePrice;

                signal += weight * r;
            }

            var indexValue = Math.Round(lastIndex.Value * signal, 2);

            ValidateIndexValue(indexValue, topAssetsWeights, whiteListAssetsMiddlePrices, lastIndex);

            var indexState = new IndexState(indexValue, whiteListAssetsMiddlePrices);

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
            await IndexStateRepository.SetAsync(indexState);

            // Save first index after reset time
            if (indexState.Value == InitialIndexValue)
            {
                await FirstStateAfterResetTimeRepository.SetAsync(indexHistory.Time);
                lock (_lastResetSync)
                {
                    _lastReset = indexHistory.Time;
                }
            }

            // Save all index info to history
            await IndexHistoryRepository.InsertAsync(indexHistory);

            // Update _topMarketCaps and _topWeights
            lock (_sync)
            {

            }

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
            TickPricePublisher.Publish(tickPrice);
        }

        private IReadOnlyList<AssetMarketCap> CalculateMarketCaps(ICollection<string> assets,
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

        private IDictionary<string, decimal> GetAssetsUsingPrices(ICollection<string> assets,
            IDictionary<string, IDictionary<string, decimal>> allPrices, IReadOnlyCollection<AssetSettings> assetsSettings)
        {
            var topAssetsUsedPrices = new Dictionary<string, decimal>();

            foreach (var asset in assets.ToList())
            {
                if (!allPrices.ContainsKey(asset) || allPrices[asset].Count == 0)
                    continue;

                var assetPrices = allPrices[asset];

                var currentMiddlePrice = GetMiddlePrice(asset, assetPrices);

                var assetSettings = assetsSettings.FirstOrDefault(x => x.AssetId == asset);

                if (assetSettings != null && assetSettings.IsDisabled)
                    currentMiddlePrice = assetSettings.Price;

                topAssetsUsedPrices[asset] = currentMiddlePrice;
            }

            return topAssetsUsedPrices;
        }

        private bool IsAllPricesArePresent(ICollection<string> assets, IDictionary<string, decimal> assetsUsingPrices)
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
                    WarningRepository.SaveAsync(new Warning(message, DateTime.UtcNow));
                    throw new InvalidOperationException(message);
                }
                
                return false;
            }

            return true;
        }

        private static decimal GetMiddlePrice(string asset, IDictionary<string, decimal> assetExchangesPrices)
        {
            if (assetExchangesPrices == null || assetExchangesPrices.Count == 0)
                throw new InvalidOperationException($"Asset '{asset}' doesn't have prices: {assetExchangesPrices.ToJson()}.");

            var prices = assetExchangesPrices.Values.OrderBy(x => x).ToList();

            if (prices.Count > 2)
            {
                prices.RemoveAt(0);
                prices.RemoveAt(prices.Count - 1);
            }

            var middlePrice = prices.Sum() / prices.Count;

            return middlePrice;
        }

        private static decimal GetPreviousMiddlePrice(string asset, IndexState lastIndex, decimal currentMiddlePrice)
        {
            if (lastIndex == null)
                return currentMiddlePrice;

            var previousPrices = lastIndex.MiddlePrices;
            return previousPrices.ContainsKey(asset)  // previous prices found in DB?
                ? previousPrices[asset]               // yes, use them
                : currentMiddlePrice;                 // no, use current
        }

        private void ValidateIndexValue(decimal indexValue, IDictionary<string, decimal> topAssetWeights,
            IDictionary<string, decimal> allAssetsMiddlePrices, IndexState lastIndex)
        {
            if (indexValue > 0)
                return;

            var message = $"Index value less or equals to 0: topAssetWeights = {topAssetWeights.ToJson()}, allAssetsPrices: {allAssetsMiddlePrices.ToJson()}, lastIndex: {lastIndex.ToJson()}.";
            WarningRepository.SaveAsync(new Warning(message, DateTime.UtcNow));
            throw new InvalidOperationException(message);
        }

        public void Stop()
        {
            _trigger.Stop();
        }

        public void Dispose()
        {
            _trigger?.Dispose();

            CoinMarketCapService?.Dispose();
        }
    }
}
