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
        private readonly ILCI10Calculator _lci10Calculator;

        public IndexHistoryController(IIndexHistoryRepository indexHistoryRepository, ILCI10Calculator lci10Calculator)
        {
            _indexHistoryRepository = indexHistoryRepository;
            _lci10Calculator = lci10Calculator;
        }

        [HttpGet("indexHistories")]
        [ProducesResponseType(typeof(IReadOnlyList<IndexHistory>), (int)HttpStatusCode.OK)]
        [ResponseCache(Duration = 30, VaryByQueryKeys = new[] { "*" })]
        public async Task<IReadOnlyList<IndexHistory>> GetLastIndexHistoriesAsync(int limit)
        {
            var domain = await _indexHistoryRepository.TakeLastAsync(limit);

            var result = Mapper.Map<IReadOnlyList<IndexHistory>>(domain);

            return result;
        }

        [HttpGet("timestamps")]
        [ProducesResponseType(typeof(IReadOnlyList<IndexHistory>), (int)HttpStatusCode.OK)]
        [ResponseCache(Duration = 60 * 10, VaryByQueryKeys = new[] { "*" })]
        public async Task<IReadOnlyList<DateTime>> GetTimestampsAsync(DateTime from, DateTime to)
        {
            var timestamps = await _indexHistoryRepository.GetTimestampsAsync(from, to);

            return timestamps;
        }

        [HttpGet]
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
