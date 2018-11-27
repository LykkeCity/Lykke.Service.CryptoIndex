using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.CryptoIndex.Domain.Models;
using Lykke.Service.CryptoIndex.Domain.Repositories;

namespace Lykke.Service.CryptoIndex.Domain.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly object _sync = new object();
        private Settings _settings;
        private Settings Settings { get { lock (_sync) { return _settings; } } set { lock (_sync) { _settings = value; } } }
        private readonly ISettingsRepository _settingsRepository;

        public SettingsService(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        public async Task<Settings> GetAsync()
        {
            if (Settings == null)
                Settings = await _settingsRepository.GetAsync();

            if (Settings == null)
            {
                Settings = new Settings(new List<string>(), new List<string>(), 10, true, TimeSpan.Zero, new List<string>());
                await _settingsRepository.InsertOrReplaceAsync(Settings);
            }

            return Settings;
        }

        public async Task SetAsync(Settings settings)
        {
            await _settingsRepository.InsertOrReplaceAsync(settings);
            Settings = settings;
        }
    }
}
