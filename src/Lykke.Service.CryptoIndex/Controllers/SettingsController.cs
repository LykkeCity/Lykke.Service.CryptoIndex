using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Service.CryptoIndex.Client.Api.LCI10;
using Lykke.Service.CryptoIndex.Domain.Services.LCI10;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.CryptoIndex.Controllers
{
    [Route("/api/lci10/[controller]")]
    public class SettingsController : Controller, ISettingsApi
    {
        private readonly ISettingsService _settingsService;

        public SettingsController(ISettingsService settingsService)
        {
            _settingsService = settingsService;
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
    }
}
