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
        private bool isReset;
        private readonly object _sync = new object();
        private readonly List<AssetMarketCap> _allMarketCaps;
        private readonly List<AssetMarketCap> _topMarketCaps;
        private readonly IDictionary<string, decimal> _topAssetsWeights;
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
        private IReadOnlyList<AssetMarketCap> TopMarketCaps { get { lock(_sync) { return _topMarketCaps.ToList(); } } }
        private IDictionary<string, decimal> TopWeights { get { lock (_sync) { return _topAssetsWeights.Clone(); } } }

        public IndexCalculator(string indexName, TimeSpan indexCalculationInterval, ILogFactory logFactory)
        {
            _startedAt = DateTime.UtcNow;
            _lastRebuild = DateTime.UtcNow.Date;
            _allMarketCaps = new List<AssetMarketCap>();
            _topMarketCaps = new List<AssetMarketCap>();
            _topAssetsWeights = new ConcurrentDictionary<string, decimal>();

            _indexName = indexName;
            _trigger = new TimerTrigger(nameof(IndexCalculator), indexCalculationInterval, logFactory, TimerHandler);

            _log = logFactory.CreateLog(this);
        }

        public void Start()
        {
            Initialize(); // top assets and weights from the last history state

            _trigger.Start();
        }

        public async Task<IReadOnlyDictionary<string, decimal>> GetAllAssetsMarketCapsAsync()
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
                isReset = true;
            }
        }

        public async Task Rebuild()
        {
            _log.Info("Started rebuilding...");

            _lastRebuild = DateTime.UtcNow.Date;

            await RefreshCoinMarketCapData();

            await CalculateWeights();

            _log.Info("Finished rebuilding.");
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

        private async Task CalculateWeights()
        {
            if (!_allMarketCaps.Any())
                return;

            _log.Info("Calculating weights...");

            // Get top 100 market caps
            List<AssetMarketCap> allMarketCaps;
            lock (_sync)
            {
                allMarketCaps = _allMarketCaps.ToList();
            }

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

        private async Task TimerHandler(ITimerTrigger timer, TimerTriggeredHandlerArgs args, CancellationToken ct)
        {
            try
            {
                if (isReset || _lastRebuild.Date < DateTime.UtcNow.Date && DateTime.UtcNow.TimeOfDay > Settings.RebuildTime)
                {
                    lock (_sync)
                    {
                        isReset = false;
                    }

                    await Rebuild();
                }

                await CalculateThenSaveAndPublish();
            }
            catch (Exception e)
            {
                _log.Warning("Somethings went wrong while index calculation.", e);
            }
        }

        private void Initialize()
        {
            _log.Info("Initializing last state from history if needed...");

            try
            {
                bool isCoinMarketCapDataPresent;

                lock (_sync)
                {
                    isCoinMarketCapDataPresent = _allMarketCaps.Any();
                }

                if (!isCoinMarketCapDataPresent)
                    RefreshCoinMarketCapData().GetAwaiter().GetResult();

                lock (_sync)
                {
                    if (_topAssetsWeights.Any() || _topMarketCaps.Any())
                    {
                        _log.Info($"Skipping initializing previous state, top assets weights count is {_topAssetsWeights.Count}, top market caps count is {_topMarketCaps.Count}.");
                        return;
                    }    

                    var lastIndexHistory = IndexHistoryRepository.TakeLastAsync(1).GetAwaiter().GetResult().SingleOrDefault();
                    if (lastIndexHistory == null)
                    {
                        _log.Info("Skipping initializing previous state, last index history is empty.");
                        return;
                    }

                    // Combined - weights are from previous index, supplies are from actual CoinMarketCap data.
                    var topMarketCapsCombined = new List<AssetMarketCap>();
                    foreach (var marketCap in lastIndexHistory.MarketCaps)
                    {
                        var circulatingSupply = _allMarketCaps.Single(x => x.Asset == marketCap.Asset).CirculatingSupply;
                        var combined = new AssetMarketCap(marketCap.Asset, marketCap.MarketCap, circulatingSupply);
                        topMarketCapsCombined.Add(combined);
                    }

                    _topMarketCaps.AddRange(topMarketCapsCombined);
                    lastIndexHistory.Weights.ForEach(x => _topAssetsWeights.Add(x.Key, x.Value));

                    _log.Info("Initialized previous weights and market caps from history.");
                }
            }
            catch (Exception e)
            {
                _log.Warning("Can't initialize last state from history.", e);
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
            var topAssets = topWeights.Keys.OrderBy(x => x).ToList();
            var lastIndex = await IndexStateRepository.GetAsync();
            var allAssetsPrices = await TickPricesService.GetPricesAsync(sources);
            var assetsSettings = settings.AssetsSettings;

            var topAssetsUsingPrices = GetTopAssetsUsingPrices(topAssets, allAssetsPrices, assetsSettings);

            // If just started and prices are not present yet, then skip.
            // If started more then {_waitForTopAssetsPricesFromStart} ago then write warning to DB and log.
            if (!IsAllPricesArePresent(topAssets, topAssetsUsingPrices))
            {
                _log.Info($"Skipped calculating index because some prices are not present yet, waiting for them for {_waitForTopAssetsPricesFromStart.TotalMinutes} minutes since start.");
                return;
            }

            // Get current index state
            var indexState = await CalculateIndex(lastIndex, topWeights, topAssetsUsingPrices);

            // if there was a reset then skip until next iteration which will have initial state
            if (indexState.Value != InitialIndexValue && State == null)
            {
                _log.Info($"Skipped saving and publishing index because of reset - previous state is null and current index not equals {InitialIndexValue}.");
                return;
            }
            
            var indexHistory = new IndexHistory(indexState.Value, TopMarketCaps, topWeights, allAssetsPrices, topAssetsUsingPrices, DateTime.UtcNow, assetsSettings);

            await Save(indexState, indexHistory);

            await Publish(indexHistory, assetsSettings);

            _log.Info($"Finished calculating index for {topWeights.Count} assets, value: {indexState.Value}.");
        }

        private IDictionary<string, decimal> GetTopAssetsUsingPrices(
            ICollection<string> topAssets,
            IDictionary<string, IDictionary<string, decimal>> allAssetsPrices,
            IReadOnlyCollection<AssetSettings> assetsSettings)
        {
            var topAssetsUsedPrices = new Dictionary<string, decimal>();

            foreach (var asset in topAssets.ToList())
            {
                if (!allAssetsPrices.ContainsKey(asset) || allAssetsPrices[asset].Count == 0)
                    continue;

                var assetPrices = allAssetsPrices[asset];

                var currentMiddlePrice = GetMiddlePrice(asset, assetPrices);

                var assetSettings = assetsSettings.FirstOrDefault(x => x.AssetId == asset);

                if (assetSettings != null && assetSettings.IsDisabled)
                    currentMiddlePrice = assetSettings.Price;

                topAssetsUsedPrices[asset] = currentMiddlePrice;
            }

            return topAssetsUsedPrices;
        }

        private async Task<IndexState> CalculateIndex(IndexState lastIndex, IDictionary<string, decimal> topAssetsWeights,
            IDictionary<string, decimal> whiteListAssetsMiddlePrices)
        {
            if (lastIndex == null)
            {
                await Rebuild();
                
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
                await FirstStateAfterResetTimeRepository.SetAsync(indexHistory.Time);

            // Save all index info to history
            await IndexHistoryRepository.InsertAsync(indexHistory);
        }

        private async Task Publish(IndexHistory indexHistory, IReadOnlyList<AssetSettings> assetsSettings)
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

        private bool IsAllPricesArePresent(ICollection<string> topAssets,
            IDictionary<string, decimal> topAssetsUsingPrices)
        {
            if (!topAssets.Any())
                return false;

            if (!topAssetsUsingPrices.Any())
                return false;

            var topAssetsWoPrices = new List<string>();
            foreach (var topAsset in topAssets)
            {
                if (!topAssetsUsingPrices.ContainsKey(topAsset))
                    topAssetsWoPrices.Add(topAsset);
            }

            if (topAssetsWoPrices.Any())
            {
                var message = $"Some top assets in index don't have prices: {topAssetsWoPrices.ToJson()}.";

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
