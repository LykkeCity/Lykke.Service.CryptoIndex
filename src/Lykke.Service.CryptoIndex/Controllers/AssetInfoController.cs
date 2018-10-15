using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Service.CryptoIndex.Client.Api.LCI10;
using Lykke.Service.CryptoIndex.Client.Models.LCI10;
using Lykke.Service.CryptoIndex.Domain.LCI10;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.CryptoIndex.Controllers
{
    [Route("/api/lci10/[controller]")]
    public class AssetInfoController : Controller, IAssetInfoApi
    {
        private readonly ILCI10Calculator _lci10Calculator;

        public AssetInfoController(ILCI10Calculator lci10Calculator)
        {
            _lci10Calculator = lci10Calculator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(AssetInfo), (int)HttpStatusCode.OK)]
        public async Task<AssetInfo> GetAssetInfoAsync(string asset)
        {
            if (string.IsNullOrWhiteSpace(asset))
                throw new ValidationApiException(HttpStatusCode.NotFound, "'asset' argument is null or empty.");

            var marketCap = await _lci10Calculator.GetAssetMarketCapAsync(asset);
            var prices = await _lci10Calculator.GetAssetPricesAsync(asset);

            var result = new AssetInfo
            {
                Asset = asset,
                MarketCap = marketCap,
                Prices = (IReadOnlyDictionary<string, decimal>)prices
            };

            return result;
        }
    }
}
