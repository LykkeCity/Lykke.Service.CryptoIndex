using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lykke.Service.CryptoIndex.Client.Api.LCI10;
using Lykke.Service.CryptoIndex.Client.Models.LCI10;
using Lykke.Service.CryptoIndex.Domain.Repositories.LCI10;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.CryptoIndex.Controllers
{
    [Route("/api/[controller]")]
    public class PublicController : Controller, IPublicApi
    {
        private readonly IIndexHistoryRepository _indexHistoryRepository;
        private readonly IIndexStateRepository _indexStateRepository;

        public PublicController(IIndexHistoryRepository indexHistoryRepository, IIndexStateRepository indexStateRepository)
        {
            _indexHistoryRepository = indexHistoryRepository;
            _indexStateRepository = indexStateRepository;
        }

        [HttpGet("twoTickPrices")]
        [ProducesResponseType(typeof(TwoTickPrices), (int)HttpStatusCode.OK)]
        [ResponseCache(Duration = 30, VaryByQueryKeys = new[] { "*" })]
        public async Task<TwoTickPrices> GetTwoTickPricesAsync()
        {
            var indexHistories = (await _indexHistoryRepository.TakeLastAsync(2)).OrderByDescending(x => x.Time).ToList();

            var result = new TwoTickPrices(indexHistories[0].Value, indexHistories[1].Value);

            return result;
        }
    }
}
