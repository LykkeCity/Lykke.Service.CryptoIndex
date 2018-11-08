using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Service.CryptoIndex.Client.Api;
using Lykke.Service.CryptoIndex.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.CryptoIndex.Controllers
{
    [Route("/api/[controller]")]
    public class PublicController : Controller, IPublicApi
    {
        private readonly IIndexHistoryRepository _indexHistoryRepository;
        private readonly IIndexStateRepository _indexStateRepository;
        private readonly IFirstStateAfterResetTimeRepository _firstStateAfterResetTimeRepository;

        public PublicController(IIndexHistoryRepository indexHistoryRepository, IIndexStateRepository indexStateRepository,
            IFirstStateAfterResetTimeRepository firstStateAfterResetTimeRepository)
        {
            _indexHistoryRepository = indexHistoryRepository;
            _indexStateRepository = indexStateRepository;
            _firstStateAfterResetTimeRepository = firstStateAfterResetTimeRepository;
        }

        [HttpGet("indices")]
        [ProducesResponseType(typeof(IReadOnlyList<(DateTime, decimal)>), (int)HttpStatusCode.OK)]
        [ResponseCache(Duration = 3, VaryByQueryKeys = new[] { "*" })]
        public async Task<IReadOnlyList<(DateTime, decimal)>> GetIndexHistoriesAsync(DateTime from, DateTime to)
        {
            var firstStateAfterResetTime = await _firstStateAfterResetTimeRepository.GetAsync();

            if (firstStateAfterResetTime.HasValue && firstStateAfterResetTime > from)
                from = firstStateAfterResetTime.Value;

            var domain = await _indexHistoryRepository.GetAsync(from, to);

            var result = domain.Select(x => (x.Time, x.Value)).ToList();

            return result;
        }

        [HttpGet("indices/upToDate")]
        [ProducesResponseType(typeof(IReadOnlyList<(DateTime, decimal)>), (int)HttpStatusCode.OK)]
        [ResponseCache(Duration = 3, VaryByQueryKeys = new[] { "*" })]
        public async Task<IReadOnlyList<(DateTime, decimal)>> GetIndexHistoriesAsync(DateTime to, int limit)
        {
            var result = await _indexHistoryRepository.GetUpToDateAsync(to, limit);
            
            return result;
        }

        [HttpGet("index/current")]
        [ProducesResponseType(typeof((DateTime, decimal)), (int)HttpStatusCode.OK)]
        [ResponseCache(Duration = 3, VaryByQueryKeys = new[] { "*" })]
        public async Task<(DateTime, decimal)> GetCurrentAsync()
        {
            var result = (await _indexHistoryRepository.TakeLastAsync(1)).SingleOrDefault();

            if (result == null)
                throw new ValidationApiException(HttpStatusCode.NotFound, "Current index value is not found.");

            return (result.Time, result.Value);
        }
    }
}
