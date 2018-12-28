﻿using System;
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
        private readonly string _indexName;

        private readonly object _sync = new object();
        private readonly List<AssetMarketCap> _allMarketCaps;
        private readonly List<string> _topAssets;
        private DateTime _lastRebuild;
        private bool _rebuildNeeded;

        private readonly object _syncLastReset = new object();
        private DateTime? _lastReset;
        private readonly object _syncLastIndexHistory = new object();
        private IndexHistory _lastIndexHistory;

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
            _lastRebuild = DateTime.UtcNow.Date;
            _allMarketCaps = new List<AssetMarketCap>();
            _topAssets = new List<string>();

            _indexName = indexName;
            _trigger = new TimerTrigger(nameof(IndexCalculator), indexCalculationInterval, logFactory, TimerHandlerAsync);

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

        public async Task ResetAsync()
        {
            // clear latest state
            await _indexStateRepository.Clear();

            lock (_sync)
            {
                _rebuildNeeded = true;
            }
        }

        public void Rebuild()
        {
            lock (_sync)
            {
                _rebuildNeeded = true;
            }
        }

        public DateTime? GetLastReset()
        {
            lock (_syncLastReset)
                return _lastReset;
        }

        public IndexHistory GetLastIndexHistory()
        {
            lock (_syncLastIndexHistory)
                return _lastIndexHistory;
        }


        private void Initialize()
        {
            _log.Info("Initializing last state from history if needed...");

            lock(_syncLastReset)
                _lastReset = _firstStateAfterResetTimeRepository.GetAsync().GetAwaiter().GetResult();

            // Initialize _allMarketCaps
            RefreshCoinMarketCapDataAsync().GetAwaiter().GetResult();

            // Restore top assets from DB
            var lastIndexHistory = _indexHistoryRepository.TakeLastAsync(1).GetAwaiter().GetResult().SingleOrDefault();
            // if found then restore _topAssets (constituents)
            if (lastIndexHistory != null)
            {
                lock (_syncLastIndexHistory)
                    _lastIndexHistory = lastIndexHistory;

                lock (_sync)
                    _topAssets.AddRange(lastIndexHistory.Weights.Keys);

                _log.Info("Initialized previous weights and market caps from history.");
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
            _log.Info("Rebuild top asset....");

            var settings = Settings;

            // Get top 100 market caps
            List<AssetMarketCap> allMarketCaps;
            lock (_sync)
                allMarketCaps = _allMarketCaps.ToList();

            // Get white list supplies
            var whiteListSupplies = new Dictionary<string, decimal>();
            settings.Assets.ForEach(x => whiteListSupplies.Add(x, allMarketCaps.Single(mk => mk.Asset == x).CirculatingSupply));

            // Get white list prices
            var sources = settings.Sources.ToList();
            var whiteListAssets = whiteListSupplies.Select(x => x.Key).ToList();
            var allAssetsPrices = await _tickPricesService.GetPricesAsync(sources);
            var assetsSettings = settings.AssetsSettings;
            var whiteListUsingPrices = GetAssetsUsingPrices(whiteListAssets, allAssetsPrices, assetsSettings);

            if (!ArePricesPresentForAllAssets(whiteListAssets, whiteListUsingPrices))
                return;

            // Calculate white list market caps
            var whiteListMarketCaps = CalculateMarketCaps(whiteListAssets, whiteListSupplies, whiteListUsingPrices);

            // Calculate white list weights
            var whiteListWeights = CalculateWeightsOrderedByDesc(whiteListMarketCaps);

            // Get top weights
            var topWeights = whiteListWeights
                .Take(Settings.TopCount)
                .ToDictionary();

            lock (_sync)
            {
                // Refresh top assets
                _topAssets.Clear();
                _topAssets.AddRange(topWeights.Keys);

                _lastRebuild = DateTime.UtcNow.Date;

                _rebuildNeeded = false;

                _log.Info($"Finished rebuilding top assets, count - {_topAssets.Count}.");
            }
        }

        private async Task TimerHandlerAsync(ITimerTrigger timer, TimerTriggeredHandlerArgs args, CancellationToken ct)
        {
            _log.Info("Timer handler started...");

            try
            {
                // Refresh CoinMarketCap data and rebuild constituents if needed
                bool rebuildNeeded;
                lock (_sync)
                {
                    var lastRebuildWasYesterday = _lastRebuild.Date < DateTime.UtcNow.Date;
                    var itIsTimeToRebuild = DateTime.UtcNow.TimeOfDay > Settings.RebuildTime;
                    rebuildNeeded = _rebuildNeeded|| lastRebuildWasYesterday && itIsTimeToRebuild;
                }
                if (rebuildNeeded)
                {
                    await RefreshCoinMarketCapDataAsync();

                    await RebuildTopAssetsAsync();
                }

                // Calculate new index
                await CalculateThenSaveAndPublishAsync();
            }
            catch (Exception e)
            {
                _log.Warning("Somethings went wrong in timer handler.", e);
            }

            _log.Info("Timer handler finished.");
        }

        private async Task CalculateThenSaveAndPublishAsync()
        {
            _log.Info("Started calculating index...");

            var settings = Settings;

            var whiteListAssets = settings.Assets;
            if (!whiteListAssets.Any())
            {
                _log.Info("There are no assets in the white list, skipped index calculation.");
                return;
            }

            IReadOnlyCollection<string> topAssets;
            IReadOnlyCollection<AssetMarketCap> allMarketCaps;
            lock (_sync)
            {
                allMarketCaps = _allMarketCaps.ToList();
                topAssets = _topAssets; // Must be obtained from _topAssets (daily rebuild changes it)
            }
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
            if (!ArePricesPresentForAllAssets(topAssets, topUsingPrices))
                return;

            // Auto freeze
            AutoFreezeIfNeeded(topAssets, topUsingPrices, lastIndex, settings);

            // Recalculate top weights
            var topSupplies = new Dictionary<string, decimal>();
            var topMarketCaps = allMarketCaps.Where(x => topAssets.Contains(x.Asset)).ToList();
            foreach (var mc in topMarketCaps)
                topSupplies.Add(mc.Asset, mc.CirculatingSupply);

            var calculatedTopMarketCaps = CalculateMarketCaps(topAssets, topSupplies, topUsingPrices);

            var calculatedTopWeights = CalculateWeightsOrderedByDesc(calculatedTopMarketCaps);

            // Calculate current index state
            var indexState = CalculateIndex(lastIndex, calculatedTopWeights, topUsingPrices);

            // Calculate current index history element
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

                lock (_syncLastReset)
                    _lastReset = indexHistory.Time;

                _log.Info($"Reset at: {indexHistory.Time.ToIsoDateTime()}.");
            }

            // Skip if changed to 'disabled'
            if (!Settings.Enabled)
            {
                _log.Info($"Skipped saving and publishing index because {nameof(Settings)}.{nameof(Settings.Enabled)} = {Settings.Enabled}.");
                return;
            }

            lock (_syncLastIndexHistory)
                _lastIndexHistory = indexHistory;

            await SaveAsync(indexState, indexHistory);

            Publish(indexHistory);

            _log.Info($"Finished calculating index for {calculatedTopMarketCaps.Count} assets, value: {indexState.Value}.");
        }

        private IndexState CalculateIndex(IndexState lastIndex, IDictionary<string, decimal> topAssetsWeights,
            IDictionary<string, decimal> topUsingPrices)
        {
            if (lastIndex == null)
                return new IndexState(InitialIndexValue, topUsingPrices);

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

            var indexState = new IndexState(indexValue, topUsingPrices);

            return indexState;
        }

        private async Task SaveAsync(IndexState indexState, IndexHistory indexHistory)
        {
            // Save index state for the next execution
            await _indexStateRepository.SetAsync(indexState);

            // Save index history
            await _indexHistoryRepository.InsertAsync(indexHistory);
        }

        private void Publish(IndexHistory indexHistory)
        {
            var assetsInfo = new List<AssetInfo>();
            var frozenAssets = indexHistory.AssetsSettings.Where(x => x.IsDisabled).Select(x => x.AssetId).ToList();
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
                throw new InvalidOperationException("Can't calculate weights, some data are missed. " +
                                                    $"Assets: {assets.ToJson()}. " +
                                                    $"Supplies: {supplies.Select(x => x.Key).ToList().ToJson()}. " +
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

                assetWeight = Math.Round(assetWeight, 8);

                weights.Add((marketCap.Asset, assetWeight));
            }

            weights = weights.OrderByDescending(x => x.Weight).ToList();

            var result = new Dictionary<string, decimal>();
            foreach (var w in weights)
                result.Add(w.Asset, w.Weight);

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

                // if frozen
                if (assetSettings != null && assetSettings.IsDisabled)
                    currentMiddlePrice = assetSettings.Price;

                topAssetsUsedPrices[asset] = currentMiddlePrice;
            }

            return topAssetsUsedPrices;
        }

        private void AutoFreezeIfNeeded(IReadOnlyCollection<string> topAssets,
            IDictionary<string, decimal> topUsingPrices, IndexState lastIndex, Settings settings)
        {
            if (lastIndex == null || settings.AutoFreezeChangePercents == default(decimal))
                return;

            // clone assets settings
            var newAssetsSettings = settings.AssetsSettings.ToList();

            foreach (var asset in topAssets)
            {
                var assetSettings = newAssetsSettings.SingleOrDefault(x => x.AssetId == asset);

                // if already disabled then skip
                if (assetSettings != null && assetSettings.IsDisabled)
                    continue;

                // calculate change
                var currentPrice = topUsingPrices[asset];
                var previousPrice = Utils.GetPreviousMiddlePrice(asset, lastIndex, currentPrice);
                var changePercents = Math.Abs((previousPrice - currentPrice) / previousPrice * 100);

                // if change is not big enough then skip
                if (changePercents < settings.AutoFreezeChangePercents)
                {
                    // just write a warning w/o auto freeze if change is more than half of AutoFreezeChangePercents
                    var halfAutoFreezeChangePercents = settings.AutoFreezeChangePercents / 2;
                    if (changePercents > halfAutoFreezeChangePercents)
                    {
                        var warningMsg = $"One time change more than {Math.Round(halfAutoFreezeChangePercents, 2)} percents for {asset}.";
                        _log.Warning(warningMsg);
                        _warningRepository.SaveAsync(new Warning(warningMsg, DateTime.UtcNow));
                    }

                    continue;
                }

                topUsingPrices[asset] = previousPrice;

                // if there was setting for current asset already then remove it
                if (assetSettings != null)
                    newAssetsSettings.Remove(newAssetsSettings.Single(x => x.AssetId == asset));

                // create new asset setting
                assetSettings = new AssetSettings(asset, previousPrice, true, true);
                newAssetsSettings.Add(assetSettings);

                // save new asset settings
                settings.AssetsSettings = newAssetsSettings;
                _settingsService.SetAsync(settings).GetAwaiter().GetResult();

                var message = $"Asset became frozen: {asset}.";
                _log.Warning(message);
                _warningRepository.SaveAsync(new Warning(message, DateTime.UtcNow));
            }
        }

        private bool ArePricesPresentForAllAssets(IReadOnlyCollection<string> assets, IDictionary<string, decimal> assetsUsingPrices)
        {
            if (!assets.Any())
                return true;
            
            var assetsWoPrices = new List<string>();
            foreach (var asset in assets)
            {
                if (!assetsUsingPrices.ContainsKey(asset))
                    assetsWoPrices.Add(asset);
            }

            if (assetsWoPrices.Any())
            {
                var message = $"Some assets don't have prices: {assetsWoPrices.ToJson()}.";
                _log.Warning(message);
                _warningRepository.SaveAsync(new Warning(message, DateTime.UtcNow));

                return false;
            }

            return true;
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
