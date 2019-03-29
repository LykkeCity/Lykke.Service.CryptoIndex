using System.Net;
using System.Threading.Tasks;
using Lykke.Service.CryptoIndex.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.CryptoIndex.Controllers
{
    [Route("/api/[controller]")]
    public class TestController : Controller
    {
        private readonly IIndexCalculator _indexCalculator;

        public TestController(IIndexCalculator indexCalculator)
        {
            _indexCalculator = indexCalculator;
        }

        [HttpGet("checkForNewAssets")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        public async Task GetAssetsAsync()
        {
            await _indexCalculator.CheckForNewAssets();
        }
    }
}
