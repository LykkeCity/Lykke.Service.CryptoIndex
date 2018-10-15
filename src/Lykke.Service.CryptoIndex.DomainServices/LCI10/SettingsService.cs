﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.CryptoIndex.Domain.LCI10.Settings;

namespace Lykke.Service.CryptoIndex.DomainServices.LCI10
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
                Settings = new Settings(new List<string>(), new List<string>());
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
