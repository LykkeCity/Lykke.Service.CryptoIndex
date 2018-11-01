using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.CryptoIndex.Client.Api;
using Lykke.Service.CryptoIndex.Domain.Repositories;
using Lykke.Service.CryptoIndex.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using IndexHistory = Lykke.Service.CryptoIndex.Client.Models.IndexHistory;

namespace Lykke.Service.CryptoIndex.Controllers
{
    [Route("/api/[controller]")]
    public class IndexHistoryController : Controller, IIndexHistoryApi
    {
        private readonly IIndexHistoryRepository _indexHistoryRepository;
        private readonly IFirstStateAfterResetTimeRepository _firstStateAfterResetTimeRepository;
        private readonly IIndexCalculator _indexCalculator;

        public IndexHistoryController(IIndexHistoryRepository indexHistoryRepository,
            IFirstStateAfterResetTimeRepository firstStateAfterResetTimeRepository,
            IIndexCalculator indexCalculator)
        {
            _indexHistoryRepository = indexHistoryRepository;
            _firstStateAfterResetTimeRepository = firstStateAfterResetTimeRepository;
            _indexCalculator = indexCalculator;
        }

        [HttpGet("indexHistories")]
        [ProducesResponseType(typeof(IReadOnlyList<IndexHistory>), (int)HttpStatusCode.OK)]
        public async Task<IReadOnlyList<IndexHistory>> GetLastIndexHistoriesAsync(int limit)
        {
            var firstStateTime = await _firstStateAfterResetTimeRepository.GetAsync();
            var domain = await _indexHistoryRepository.TakeLastAsync(limit, firstStateTime);

            var result = Mapper.Map<IReadOnlyList<IndexHistory>>(domain);

            return result;
        }

        [HttpGet("timestamps")]
        [ProducesResponseType(typeof(IReadOnlyList<IndexHistory>), (int)HttpStatusCode.OK)]
        public async Task<IReadOnlyList<DateTime>> GetTimestampsAsync(DateTime from, DateTime to)
        {
            var firstStateAfterResetTime = await _firstStateAfterResetTimeRepository.GetAsync();

            if (firstStateAfterResetTime.HasValue && firstStateAfterResetTime > from)
                from = firstStateAfterResetTime.Value;

            var timestamps = await _indexHistoryRepository.GetTimestampsAsync(from, to);

            return timestamps;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IndexHistory), (int)HttpStatusCode.OK)]
        [ResponseCache(Duration = 60 * 60, VaryByQueryKeys = new[] { "*" })]
        public async Task<IndexHistory> GetAsync(DateTime timestamp)
        {
            var domain = await _indexHistoryRepository.GetAsync(timestamp);

            var result = Mapper.Map<IndexHistory>(domain);

            return result;
        }
    }
}
