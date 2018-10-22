using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lykke.Service.CryptoIndex.Client.Api;
using Lykke.Service.CryptoIndex.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.CryptoIndex.Controllers
{
    [Route("/api/[controller]")]
    public class MarketCapsController : Controller, IMarketCapsApi
    {
        private readonly IMarketCapitalizationService _marketCapitalizationService;

        public MarketCapsController(IMarketCapitalizationService marketCapitalizationService)
        {
            _marketCapitalizationService = marketCapitalizationService;
        }

        [HttpGet("assets")]
        [ProducesResponseType(typeof(IReadOnlyList<string>), (int)HttpStatusCode.OK)]
        public async Task<IReadOnlyList<string>> GetAssetsAsync()
        {
            var result = await _marketCapitalizationService.GetAllAsync();

            return result.Select(x => x.Asset).ToList();
        }
    }
}
