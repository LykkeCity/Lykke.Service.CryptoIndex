using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.CryptoIndex.Client.Api.LCI10;
using Lykke.Service.CryptoIndex.Domain.LCI10.IndexHistory;
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

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<IndexHistory>), (int)HttpStatusCode.OK)]
        public async Task<IReadOnlyList<IndexHistory>> GetIndexHistoryAsync(DateTime from, DateTime to)
        {
            var domain = await _indexHistoryRepository.GetAsync(from, to);

            var result = Mapper.Map<IReadOnlyList<IndexHistory>>(domain);

            return result;
        }
    }
}
