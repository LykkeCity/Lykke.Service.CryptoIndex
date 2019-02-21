using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.CryptoIndex.Domain.Handlers;
using Lykke.Service.CryptoIndex.Domain.Models;

namespace Lykke.Service.CryptoIndex.Domain.Services
{
    public class TickPricesService : ITickPricesService, ITickPriceHandler
    {
        private const string Usd = "USD";
        private readonly object _sync = new object();
        private readonly IDictionary<string, List<TickPrice>> _assetsTickPricesCache;
        private readonly IDictionary<string, List<AssetPrice>> _assetsPricesCache;
        private readonly ISettingsService _settingsService;
        private readonly ILog _log;

        public TickPricesService(ISettingsService settingsService, ILogFactory logFactory)
        {
            // source 
            _assetsTickPricesCache = new ConcurrentDictionary<string, List<TickPrice>>();
            _assetsPricesCache = new ConcurrentDictionary<string, List<AssetPrice>>();

            _settingsService = settingsService;
            _log = logFactory.CreateLog(this);
        }

        public async Task HandleAsync(TickPrice tickPrice)
        {
            if (tickPrice.MiddlePrice == null)
                return;

            // xxx/usd
            bool shallBeIncluded = tickPrice.AssetPair.EndsWith(Usd);
            
            AssetPrice assetPrice = new AssetPrice();

            if (shallBeIncluded)
            {
                assetPrice.Asset = tickPrice.AssetPair.Replace(Usd, string.Empty);
                assetPrice.CrossAsset = Usd;
                assetPrice.Source = tickPrice.Source;
                assetPrice.Price = tickPrice.MiddlePrice.Value;
            }   

            // xxx/cross & cross/usd
            if (!shallBeIncluded)
            {
                IReadOnlyList<string> crossAssets = (await _settingsService.GetAsync()).CrossAssets;

                foreach (string cross in crossAssets)
                {
                    // if there no cross/usd yet then skip
                    lock (_sync)
                        if (_assetsTickPricesCache.ContainsKey(cross))
                            continue;

                    // xxx/cross
                    if (!tickPrice.AssetPair.EndsWith(cross))
                        continue;

                    assetPrice.Asset = tickPrice.AssetPair.Replace(cross, string.Empty);
                    assetPrice.CrossAsset = cross;
                    assetPrice.Source = tickPrice.Source;

                    // cross/usd
                    TickPrice crossUsd;

                    lock (_sync)
                    {
                        IList<TickPrice> crossAssetTickPrices = _assetsTickPricesCache[cross];

                        crossUsd = crossAssetTickPrices.SingleOrDefault(x =>
                            x.AssetPair.StartsWith(cross)
                            && x.AssetPair.EndsWith(Usd)
                            && x.Source == tickPrice.Source); // must be the same Source
                    }

                    if (crossUsd != null)
                    {
                        decimal? crossAsk = tickPrice.Ask * crossUsd.Ask;
                        decimal? crossBid = tickPrice.Bid * crossUsd.Bid;

                        var crossTickPrice = new TickPrice(string.Empty, string.Empty, crossBid, crossAsk, DateTime.UtcNow);

                        decimal? middlePrice = crossTickPrice.MiddlePrice;

                        if (middlePrice == null)
                            continue;

                        // calculated cross middle price
                        assetPrice.Price = middlePrice.Value;

                        shallBeIncluded = true;

                        break;
                    }
                }
            }

            if (!shallBeIncluded)
                return;

            lock (_sync)
            {
                AddOrUpdateTickPrice(assetPrice.Asset, tickPrice);

                AddOrUpdateAssetPrice(assetPrice);
            }
        }

        private void AddOrUpdateTickPrice(string asset, TickPrice tickPrice)
        {
            // new list
            if (!_assetsTickPricesCache.ContainsKey(asset))
            {
                var newList = new List<TickPrice> { tickPrice };

                _assetsTickPricesCache[asset] = newList;
            }
            // existed list
            else
            {
                var exchangesPrices = _assetsTickPricesCache[asset];

                // Replace by source and assetPair
                Predicate<TickPrice> existedTickPrices = x =>
                    x.AssetPair == tickPrice.AssetPair
                    && x.Source == tickPrice.Source;

                exchangesPrices.RemoveAll(existedTickPrices);

                exchangesPrices.Add(tickPrice);
            }
        }

        private void AddOrUpdateAssetPrice(AssetPrice assetPrice)
        {
            var asset = assetPrice.Asset;

            // asset not existed
            if (!_assetsPricesCache.ContainsKey(asset))
            {
                var newAssetPrices = new List<AssetPrice> { assetPrice };

                _assetsPricesCache[asset] = newAssetPrices;
            }
            // asset existed
            else
            {
                List<AssetPrice> assetPrices = _assetsPricesCache[asset];

                // Replace by asset, crossAsset and source
                Predicate<AssetPrice> existedAssetPrices = x =>
                    x.Asset == assetPrice.Asset
                    && x.CrossAsset == assetPrice.CrossAsset
                    && x.Source == assetPrice.Source;

                assetPrices.RemoveAll(existedAssetPrices);

                assetPrices.Add(assetPrice);
            }
        }

        public IDictionary<string, IReadOnlyCollection<TickPrice>> GetTickPrices(IReadOnlyCollection<string> sources = null)
        {
            IDictionary<string, IReadOnlyCollection<TickPrice>> result;

            lock (_sync)
                result = Clone(_assetsTickPricesCache);

            if (sources == null)
                return result;

            if (!sources.Any())
                return new Dictionary<string, IReadOnlyCollection<TickPrice>>();

            // filter out sources that not in the 'sources' argument
            var assets = result.Keys.ToList();
            foreach (var asset in assets)
            {
                var tickPrices = result[asset].Where(x => sources.Contains(x.Source)).ToList();

                result[asset] = tickPrices;
            }

            return result;
        }

        public IDictionary<string, IReadOnlyCollection<AssetPrice>> GetAssetPrices(IReadOnlyCollection<string> sources = null)
        {
            IDictionary<string, IReadOnlyCollection<AssetPrice>> result;

            lock (_sync)
                result = Clone(_assetsPricesCache);

            if (sources == null)
                return result;

            if (!sources.Any())
                return new Dictionary<string, IReadOnlyCollection<AssetPrice>>();

            // filter out sources that not in the 'sources' argument
            var assets = result.Keys.ToList();
            foreach (var asset in assets)
            {
                var tickPrices = result[asset].Where(x => sources.Contains(x.Source)).ToList();

                result[asset] = tickPrices;
            }

            return result;
        }

        private IDictionary<string, IReadOnlyCollection<T>> Clone<T>(IDictionary<string, List<T>> value)
        {
            var result = new Dictionary<string, IReadOnlyCollection<T>>();

            foreach (var assetPrices in value)
            {
                List<T> list;
                lock (_sync)
                    list = assetPrices.Value.ToList();

                result.Add(assetPrices.Key, list);
            }

            return result;
        }
    }
}
