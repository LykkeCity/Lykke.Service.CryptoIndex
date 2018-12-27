using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.CryptoIndex.Client.Api;
using Lykke.Service.CryptoIndex.Client.Models;
using Lykke.Service.CryptoIndex.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.CryptoIndex.Controllers
{
    [Route("/api/[controller]")]
    public class WarningsController : Controller, IWarningsApi
    {
        private readonly IWarningRepository _warningRepository;

        public WarningsController(IWarningRepository warningRepository)
        {
            _warningRepository = warningRepository;
        }

        [HttpGet("last")]
        [ProducesResponseType(typeof(IReadOnlyList<Warning>), (int)HttpStatusCode.OK)]
        public async Task<IReadOnlyList<Warning>> GetLastWarningsAsync(int limit)
        {
            var domain = await _warningRepository.TakeAsync(limit);

            var result = Mapper.Map<IReadOnlyList<Warning>>(domain);

            return result;
        }

        [HttpGet("history")]
        [ProducesResponseType(typeof(IReadOnlyList<Warning>), (int)HttpStatusCode.OK)]
        public async Task<IReadOnlyList<Warning>> GetHistoryAsync(DateTime from, DateTime to)
        {
            var domain = await _warningRepository.GetAsync(from, to);

            var result = Mapper.Map<IReadOnlyList<Warning>>(domain);

            return result;
        }
    }
}
