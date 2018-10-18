using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Lykke.Service.CryptoIndex.Client.Api.LCI10;
using Lykke.Service.CryptoIndex.Client.Models.LCI10;
using Lykke.Service.CryptoIndex.Domain.Services;
using Lykke.Service.CryptoIndex.Domain.Services.LCI10;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.CryptoIndex.Controllers
{
    [Route("/api/lci10/[controller]")]
    public class AssetsInfoController : Controller, IAssetsInfoApi
    {
        private readonly ILCI10Calculator _lci10Calculator;
        private readonly ITickPricesService _tickPricesService;

        public AssetsInfoController(ILCI10Calculator lci10Calculator, ITickPricesService tickPricesService)
        {
            _lci10Calculator = lci10Calculator;
            _tickPricesService = tickPricesService;
        }

        [HttpGet("all")]
        [ProducesResponseType(typeof(IReadOnlyList<AssetInfo>), (int)HttpStatusCode.OK)]
        public async Task<IReadOnlyList<AssetInfo>> GetAllAsync()
        {
            var marketCaps = await _lci10Calculator.GetAssetMarketCapAsync();
            var prices = await _tickPricesService.GetPricesAsync();

            var result = new List<AssetInfo>();

            foreach (var asset in marketCaps.Keys)
            {
                if (!prices.ContainsKey(asset))
                    continue;

                var marketCap = marketCaps[asset];
                var assetPrices = prices[asset];

                var assetInfo = new AssetInfo
                {
                    Asset = asset,
                    MarketCap = marketCap,
                    Prices = (IReadOnlyDictionary<string, decimal>)assetPrices
                };

                result.Add(assetInfo);
            }

            return result;
        }
    }
}
