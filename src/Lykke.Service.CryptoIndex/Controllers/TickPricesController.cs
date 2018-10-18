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
    public class TickPricesController : Controller, ITickPricesApi
    {
        private readonly ITickPricesService _tickPricesService;

        public TickPricesController(ITickPricesService tickPricesService)
        {
            _tickPricesService = tickPricesService;
        }

        [HttpGet("sources")]
        [ProducesResponseType(typeof(IReadOnlyList<string>), (int)HttpStatusCode.OK)]
        public async Task<IReadOnlyList<string>> GetExchangesAsync()
        {
            var prices = await _tickPricesService.GetPricesAsync();

            var result = prices.SelectMany(x => x.Value.Keys).Distinct().OrderBy(x => x).ToList();

            return result;
        }
    }
}
