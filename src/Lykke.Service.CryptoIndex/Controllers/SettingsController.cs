using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Service.CryptoIndex.Client.Api.LCI10;
using Lykke.Service.CryptoIndex.Domain.Repositories.LCI10;
using Lykke.Service.CryptoIndex.Domain.Services.LCI10;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.CryptoIndex.Controllers
{
    [Route("/api/[controller]")]
    public class SettingsController : Controller, ISettingsApi
    {
        private readonly ISettingsService _settingsService;
        private readonly IIndexStateRepository _indexStateRepository;

        public SettingsController(ISettingsService settingsService, IIndexStateRepository indexStateRepository)
        {
            _settingsService = settingsService;
            _indexStateRepository = indexStateRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(Client.Models.LCI10.Settings), (int)HttpStatusCode.OK)]
        public async Task<Client.Models.LCI10.Settings> GetAsync()
        {
            var domain = await _settingsService.GetAsync();

            var model = Mapper.Map<Client.Models.LCI10.Settings>(domain);

            return model;
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task SetAsync([FromBody] Client.Models.LCI10.Settings settings)
        {
            if (settings == null)
                throw new ValidationApiException(HttpStatusCode.NotFound, "'settings' argument is null.");

            var domain = Mapper.Map<Domain.Models.LCI10.Settings>(settings);

            await _settingsService.SetAsync(domain);
        }

        [HttpGet("reset")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task ResetAsync()
        {
            await _indexStateRepository.Clear();
        }
    }
}
