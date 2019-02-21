using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lykke.Service.CryptoIndex.Client.Api;
using Lykke.Service.CryptoIndex.Client.Models;
using Lykke.Service.CryptoIndex.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.CryptoIndex.Controllers
{
    [Route("/api/[controller]")]
    public class AssetsInfoController : Controller, IAssetsInfoApi
    {
        private readonly ISettingsService _settingsService;
        private readonly IIndexCalculator _indexCalculator;
        private readonly ITickPricesService _tickPricesService;

        public AssetsInfoController(ISettingsService settingsService, IIndexCalculator indexCalculator, ITickPricesService tickPricesService)
        {
            _settingsService = settingsService;
            _indexCalculator = indexCalculator;
            _tickPricesService = tickPricesService;
        }

        [HttpGet("all")]
        [ProducesResponseType(typeof(IReadOnlyList<AssetInfo>), (int)HttpStatusCode.OK)]
        public async Task<IReadOnlyList<AssetInfo>> GetAllAsync()
        {
            var settings = await _settingsService.GetAsync();
            var marketCaps = _indexCalculator.GetAllAssetsMarketCaps();
            var prices = _tickPricesService.GetAssetPrices(settings.Sources.ToList());

            var result = new List<AssetInfo>();

            foreach (var asset in settings.Assets)
            {
                if (!marketCaps.ContainsKey(asset))
                    continue;

                var marketCap = marketCaps[asset];

                IReadOnlyDictionary<string, decimal> assetPrices = new Dictionary<string, decimal>();
                if (prices.ContainsKey(asset))
                    assetPrices = prices[asset].ToDictionary(x => x.Source, x => x.Price);

                var assetInfo = new AssetInfo
                {
                    Asset = asset,
                    MarketCap = marketCap,
                    Prices = assetPrices
                };

                result.Add(assetInfo);
            }

            return result;
        }
    }
}
