using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.CryptoIndex.Domain;
using Lykke.Service.CryptoIndex.Domain.LCI10.Settings;
using Lykke.Service.CryptoIndex.Domain.TickPrice;

namespace Lykke.Service.CryptoIndex.Domain.Services.TickPrice
{
    public class TickPricesService : ITickPricesService, ITickPriceHandler
    {
        private const string Usd = "USD";
        private readonly IDictionary<string, IDictionary<string, decimal>> _pricesCache;
        private readonly ISettingsService _settingsService;
        private readonly ILog _log;

        public TickPricesService(ISettingsService settingsService, ILogFactory logFactory)
        {
            _pricesCache = new ConcurrentDictionary<string, IDictionary<string, decimal>>();

            _settingsService = settingsService;

            _log = logFactory.CreateLog(this);
        }

        public async Task HandleAsync(Domain.TickPrice.TickPrice tickPrice)
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

        public async Task<IDictionary<string, IDictionary<string, decimal>>> GetPricesAsync()
        {
            return _pricesCache.Clone();
        }
    }
}
