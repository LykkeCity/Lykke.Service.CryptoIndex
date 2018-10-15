using Lykke.HttpClientGenerator;
using Lykke.Service.CryptoIndex.Client.Api.LCI10;

namespace Lykke.Service.CryptoIndex.Client
{
    /// <summary>
    /// CryptoIndex API aggregating interface.
    /// </summary>
    public class CryptoIndexClient : ICryptoIndexClient
    {
        /// <inheritdoc/>
        public IAssetInfoApi AssetInfo { get; }

        /// <inheritdoc/>
        public ISettingsApi Settings { get; }

        /// <summary>C-tor</summary>
        public CryptoIndexClient(IHttpClientGenerator httpClientGenerator)
        {
            AssetInfo = httpClientGenerator.Generate<IAssetInfoApi>();
            Settings = httpClientGenerator.Generate<ISettingsApi>();
        }
    }
}
