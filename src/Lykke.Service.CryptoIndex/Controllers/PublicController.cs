using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Service.CryptoIndex.Client.Api;
using Lykke.Service.CryptoIndex.Client.Models;
using Lykke.Service.CryptoIndex.Domain.Repositories;
using Lykke.Service.CryptoIndex.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using IndexHistory = Lykke.Service.CryptoIndex.Domain.Models.IndexHistory;

namespace Lykke.Service.CryptoIndex.Controllers
{
    [Route("/api/[controller]")]
    public class PublicController : Controller, IPublicApi
    {
        private readonly IIndexHistoryRepository _indexHistoryRepository;
        private readonly IIndexStateRepository _indexStateRepository;
        private readonly IIndexCalculator _indexCalculator;

        private readonly object _sync = new object();
        private static DateTime? _lastReset;
        private static IndexHistory _midnight;

        public PublicController(IIndexHistoryRepository indexHistoryRepository, IIndexStateRepository indexStateRepository,
            IIndexCalculator indexCalculator)
        {
            _indexHistoryRepository = indexHistoryRepository;
            _indexStateRepository = indexStateRepository;
            _indexCalculator = indexCalculator;
        }

        [HttpGet("indices")]
        [ProducesResponseType(typeof(IReadOnlyList<(DateTime, decimal)>), (int)HttpStatusCode.OK)]
        [ResponseCache(Duration = 10)]
        [Obsolete]
        public async Task<IReadOnlyList<(DateTime, decimal)>> GetIndexHistoriesAsync(DateTime from, DateTime to)
        {
            return await GetChangeAsync();
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
            var resultPoints = new List<IndexHistory>();

            var from = DateTime.UtcNow.Date;

            lock (_sync)
            {
                var lastReset = _indexCalculator.GetLastReset();
                if (_midnight == null || lastReset != _lastReset)
                {
                    _lastReset = lastReset;
                    from = _lastReset.HasValue && _lastReset.Value > from ? _lastReset.Value : from;
                    _midnight = _indexHistoryRepository.GetAsync(from, DateTime.UtcNow).GetAwaiter().GetResult().FirstOrDefault();
                }

                if (_midnight != null)
                    resultPoints.Add(_midnight);
            }       

            var last = _indexCalculator.GetLastIndexHistory();

            if (last != null)
                resultPoints.Add(last);

            var result = resultPoints.Select(x => (x.Time, x.Value)).OrderBy(x => x.Time).ToList();

            return result;
        }
    }
}
