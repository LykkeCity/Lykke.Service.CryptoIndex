using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.CryptoIndex.Domain.Handlers;

namespace Lykke.Service.CryptoIndex.Domain.Services
{
    public class TickPricesService : ITickPricesService, ITickPriceHandler
    {
        private const string Usd = "USD";
        private readonly IDictionary<string, IDictionary<string, decimal>> _assetsSourcesPricesCache;
        private readonly ILog _log;

        public TickPricesService(ILogFactory logFactory)
        {
            _assetsSourcesPricesCache = new ConcurrentDictionary<string, IDictionary<string, decimal>>();

            _log = logFactory.CreateLog(this);
        }

        public async Task HandleAsync(Models.TickPrice tickPrice)
        {
            if (!tickPrice.AssetPair.ToUpper().EndsWith(Usd) || !tickPrice.Ask.HasValue && !tickPrice.Bid.HasValue)
                return;

            var asset = tickPrice.AssetPair.ToUpper().Replace(Usd, "");

            var price = tickPrice.Ask.HasValue && tickPrice.Bid.HasValue
                ? (tickPrice.Ask.Value + tickPrice.Bid.Value) / 2
                : tickPrice.Ask ?? tickPrice.Bid.Value;

            if (!_assetsSourcesPricesCache.ContainsKey(asset))
            {
                var newDictionary = new ConcurrentDictionary<string, decimal>
                {
                    [tickPrice.Source] = price
                };
                _assetsSourcesPricesCache[asset] = newDictionary;
            }
            else
            {
                var exchangesPrices = _assetsSourcesPricesCache[asset];
                exchangesPrices[tickPrice.Source] = price;
            }
        }

        public async Task<IDictionary<string, IDictionary<string, decimal>>> GetPricesAsync(ICollection<string> sources)
        {
            var result = _assetsSourcesPricesCache.Clone();

            if (sources == null || !sources.Any())
                return result;

            var assets = result.Keys.ToList();
            foreach (var asset in assets)
            {
                var sourcesPrices = result[asset];
                var existedSources = sourcesPrices.Keys.ToList();
                var sourcesToRemove = existedSources.Except(sources);
                foreach (var sourceToRemove in sourcesToRemove)
                    sourcesPrices.Remove(sourceToRemove);
            }

            return result;
        }
    }
}
