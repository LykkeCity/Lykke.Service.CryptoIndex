using Lykke.HttpClientGenerator;
using Lykke.Service.CryptoIndex.Client.Api;

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
        public IIndexHistoryApi IndexHistory { get; }

        /// <inheritdoc/>
        public IPublicApi Public { get; }

        /// <inheritdoc/>
        public ISettingsApi Settings { get; }

        /// <inheritdoc/>
        public ITickPricesApi TickPrices { get; }

        /// <inheritdoc/>
        public IWarningsApi Warnings { get; }

        /// <summary>C-tor</summary>
        public CryptoIndexClient(IHttpClientGenerator httpClientGenerator)
        {
            AssetsInfo = httpClientGenerator.Generate<IAssetsInfoApi>();
            IndexHistory = httpClientGenerator.Generate<IIndexHistoryApi>();
            Public = httpClientGenerator.Generate<IPublicApi>();
            Settings = httpClientGenerator.Generate<ISettingsApi>();
            TickPrices = httpClientGenerator.Generate<ITickPricesApi>();
            Warnings = httpClientGenerator.Generate<IWarningsApi>();
        }
    }
}
