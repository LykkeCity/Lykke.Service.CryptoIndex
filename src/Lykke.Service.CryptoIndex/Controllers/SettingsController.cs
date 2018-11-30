using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Service.CryptoIndex.Client.Api;
using Lykke.Service.CryptoIndex.Domain.Repositories;
using Lykke.Service.CryptoIndex.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.CryptoIndex.Controllers
{
    [Route("/api/[controller]")]
    public class SettingsController : Controller, ISettingsApi
    {
        private readonly ISettingsService _settingsService;
        private readonly IIndexStateRepository _indexStateRepository;
        private readonly IIndexCalculator _indexCalculator;

        public SettingsController(ISettingsService settingsService, IIndexStateRepository indexStateRepository, IIndexCalculator indexCalculator)
        {
            _settingsService = settingsService;
            _indexStateRepository = indexStateRepository;
            _indexCalculator = indexCalculator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(Client.Models.Settings), (int)HttpStatusCode.OK)]
        public async Task<Client.Models.Settings> GetAsync()
        {
            var domain = await _settingsService.GetAsync();

            var model = Mapper.Map<Client.Models.Settings>(domain);

            return model;
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task SetAsync([FromBody] Client.Models.Settings settings)
        {
            if (settings == null)
                throw new ValidationApiException(HttpStatusCode.NotFound, "'settings' argument is null.");

            var domain = Mapper.Map<Domain.Models.Settings>(settings);

            await _settingsService.SetAsync(domain);
        }

        [HttpGet("reset")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task ResetAsync()
        {
            await _indexCalculator.Reset();
        }

        [HttpGet("rebuild")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task RebuildAsync()
        {
            await _indexCalculator.Rebuild();
        }
    }
}
