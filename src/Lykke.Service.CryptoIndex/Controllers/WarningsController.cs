using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.CryptoIndex.Client.Api.LCI10;
using Lykke.Service.CryptoIndex.Client.Models.LCI10;
using Lykke.Service.CryptoIndex.Domain.Repositories.LCI10;
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

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<Warning>), (int)HttpStatusCode.OK)]
        [ResponseCache(Duration = 60 * 10, VaryByQueryKeys = new[] { "*" })]
        public async Task<IReadOnlyList<Warning>> GetWarningsAsync(DateTime from, DateTime to)
        {
            var domain = await _warningRepository.GetAsync(from, to);

            var result = Mapper.Map<IReadOnlyList<Warning>>(domain);

            return result;
        }
    }
}
