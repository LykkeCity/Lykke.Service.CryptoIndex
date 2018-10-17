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
        public IAssetsInfoApi AssetsInfo { get; }

        /// <inheritdoc/>
        public ISettingsApi Settings { get; }

        /// <inheritdoc/>
        public IIndexHistoryApi IndexHistory { get; }

        /// <inheritdoc/>
        public ITickPricesApi TickPrices { get; }

        /// <summary>C-tor</summary>
        public CryptoIndexClient(IHttpClientGenerator httpClientGenerator)
        {
            AssetsInfo = httpClientGenerator.Generate<IAssetsInfoApi>();
            Settings = httpClientGenerator.Generate<ISettingsApi>();
            IndexHistory = httpClientGenerator.Generate<IIndexHistoryApi>();
            TickPrices = httpClientGenerator.Generate<ITickPricesApi>();
        }
    }
}
