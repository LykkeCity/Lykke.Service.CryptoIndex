using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly string _indexTickPriceAssetPair;
        private readonly ISettingsRepository _settingsRepository;

        public SettingsService(ISettingsRepository settingsRepository, string indexTickPriceAssetPair)
        {
            _settingsRepository = settingsRepository;
            _indexTickPriceAssetPair = indexTickPriceAssetPair.ToUpper();
        }

        public async Task<Settings> GetAsync()
        {
            if (Settings == null)
                Settings = await _settingsRepository.GetAsync();

            if (Settings == null)
            {
                Settings = new Settings
                {
                    Sources = new List<string>(),
                    Assets = new List<string>(),
                    TopCount = 10,
                    Enabled = true,
                    RebuildTime = TimeSpan.Zero,
                    AssetsSettings = new List<AssetSettings>(),
                    AutoFreezeChangePercents = 10,
                    CrossAssets = new List<string> {"BTC", "ETH"}
                };

                await SetAsync(Settings);
            }

            // default values
            if (Settings.TopAssetsForAlert == 0)
            {
                Settings.TopAssetsForAlert = 50;
                await SetAsync(Settings);
            }

            return Settings;
        }

        public async Task SetAsync(Settings settings)
        {
            await _settingsRepository.InsertOrReplaceAsync(settings);

            Settings = settings;
        }

        public string GetIndexTickPriceAssetPairName()
        {
            return _indexTickPriceAssetPair;
        }
    }
}
