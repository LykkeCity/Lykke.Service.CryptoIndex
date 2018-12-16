using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Service.CryptoIndex.Client.Api;
using Lykke.Service.CryptoIndex.Client.Models;
using Lykke.Service.CryptoIndex.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
using IndexHistory = Lykke.Service.CryptoIndex.Domain.Models.IndexHistory;

namespace Lykke.Service.CryptoIndex.Controllers
{
    [Route("/api/[controller]")]
    public class PublicController : Controller, IPublicApi
    {
        private readonly IIndexHistoryRepository _indexHistoryRepository;
        private readonly IIndexStateRepository _indexStateRepository;
        private readonly IFirstStateAfterResetTimeRepository _firstStateAfterResetTimeRepository;

        private static readonly ConcurrentDictionary<DateTime, decimal> Cache = new ConcurrentDictionary<DateTime, decimal>();

        public PublicController(IIndexHistoryRepository indexHistoryRepository, IIndexStateRepository indexStateRepository,
            IFirstStateAfterResetTimeRepository firstStateAfterResetTimeRepository)
        {
            _indexHistoryRepository = indexHistoryRepository;
            _indexStateRepository = indexStateRepository;
            _firstStateAfterResetTimeRepository = firstStateAfterResetTimeRepository;
        }

        [HttpGet("indices")]
        [ProducesResponseType(typeof(IReadOnlyList<(DateTime, decimal)>), (int)HttpStatusCode.OK)]
        [ResponseCache(Duration = 10)]
        [Obsolete]
        public async Task<IReadOnlyList<(DateTime, decimal)>> GetIndexHistoriesAsync(DateTime from, DateTime to)
        {
            return await GetChangeAsync();
        }

        [HttpGet("indices/upToDate")]
        [ProducesResponseType(typeof(IReadOnlyList<(DateTime, decimal)>), (int)HttpStatusCode.OK)]
        [ResponseCache(Duration = 10, VaryByQueryKeys = new[] { "*" })]
        public async Task<IReadOnlyList<(DateTime, decimal)>> GetIndexHistoriesAsync(DateTime to, int limit)
        {
            var result = await _indexHistoryRepository.GetUpToDateAsync(to, limit);
            
            return result;
        }

        [HttpGet("index/current")]
        [ProducesResponseType(typeof((DateTime, decimal)), (int)HttpStatusCode.OK)]
        [ResponseCache(Duration = 10, VaryByQueryKeys = new[] { "*" })]
        [Obsolete]
        public async Task<(DateTime, decimal)> GetCurrentAsync()
        {
            var result = (await _indexHistoryRepository.TakeLastAsync(1)).SingleOrDefault();

            if (result == null)
                throw new ValidationApiException(HttpStatusCode.NotFound, "Current index value is not found.");

            return (result.Time, result.Value);
        }

        [HttpGet("index/last")]
        [ProducesResponseType(typeof(PublicIndexHistory), (int)HttpStatusCode.OK)]
        [ResponseCache(Duration = 10, VaryByQueryKeys = new[] { "*" })]
        public async Task<PublicIndexHistory> GetLastAsync()
        {
            var domain = (await _indexHistoryRepository.TakeLastAsync(1)).SingleOrDefault();

            if (domain == null)
                throw new ValidationApiException(HttpStatusCode.NotFound, "Last index value is not found.");

            var result = Mapper.Map<PublicIndexHistory>(domain);

            return result;
        }

        [HttpGet("change")]
        [ProducesResponseType(typeof(IReadOnlyList<(DateTime, decimal)>), (int)HttpStatusCode.OK)]
        [ResponseCache(Duration = 10, VaryByQueryKeys = new[] { "*" })]
        public async Task<IReadOnlyList<(DateTime, decimal)>> GetChangeAsync()
        {
            var fromMidnight = await _indexHistoryRepository.GetAsync(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddMinutes(2));
            var midnight = fromMidnight.FirstOrDefault();

            var last = (await _indexHistoryRepository.TakeLastAsync(1)).SingleOrDefault();

            var resultPoints = new List<IndexHistory>();
            if (midnight != null)
                resultPoints.Add(midnight);
            resultPoints.Add(last);

            var result = resultPoints.Select(x => (x.Time, x.Value)).OrderBy(x => x.Time).ToList();

            return result;
        }
    }
}
