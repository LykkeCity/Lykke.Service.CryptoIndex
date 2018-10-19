using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.CryptoIndex.Client.Api.LCI10;
using Lykke.Service.CryptoIndex.Domain.Repositories.LCI10;
using Microsoft.AspNetCore.Mvc;
using IndexHistory = Lykke.Service.CryptoIndex.Client.Models.LCI10.IndexHistory;

namespace Lykke.Service.CryptoIndex.Controllers
{
    [Route("/api/lci10/[controller]")]
    public class IndexHistoryController : Controller, IIndexHistoryApi
    {
        private readonly IIndexHistoryRepository _indexHistoryRepository;

        public IndexHistoryController(IIndexHistoryRepository indexHistoryRepository)
        {
            _indexHistoryRepository = indexHistoryRepository;
        }

        [HttpGet("indexHistories/{from}/{to}")]
        [ProducesResponseType(typeof(IReadOnlyList<IndexHistory>), (int)HttpStatusCode.OK)]
        [ResponseCache(Duration = 60 * 10, VaryByQueryKeys = new[] { "*" })]
        public async Task<IReadOnlyList<IndexHistory>> GetIndexHistoryAsync(DateTime from, DateTime to)
        {
            var domain = await _indexHistoryRepository.GetAsync(from, to);

            var result = Mapper.Map<IReadOnlyList<IndexHistory>>(domain);

            return result;
        }

        [HttpGet("timestamps/{from}/{to}")]
        [ProducesResponseType(typeof(IReadOnlyList<IndexHistory>), (int)HttpStatusCode.OK)]
        [ResponseCache(Duration = 60 * 10, VaryByQueryKeys = new[] { "*" })]
        public async Task<IReadOnlyList<DateTime>> GetTimestampsAsync(DateTime from, DateTime to)
        {
            var timestamps = await _indexHistoryRepository.GetTimestampsAsync(from, to);

            return timestamps;
        }

        [HttpGet("/{timestamp}")]
        [ProducesResponseType(typeof(IndexHistory), (int)HttpStatusCode.OK)]
        [ResponseCache(Duration = 60 * 10, VaryByQueryKeys = new[] { "*" })]
        public async Task<IndexHistory> GetAsync(DateTime timestamp)
        {
            var domain = await _indexHistoryRepository.GetAsync(timestamp);

            var result = Mapper.Map<IndexHistory>(domain);

            return result;
        }
    }
}
