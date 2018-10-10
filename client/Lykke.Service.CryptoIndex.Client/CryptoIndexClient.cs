using Lykke.HttpClientGenerator;

namespace Lykke.Service.CryptoIndex.Client
{
    /// <summary>
    /// CryptoIndex API aggregating interface.
    /// </summary>
    public class CryptoIndexClient : ICryptoIndexClient
    {
        // Note: Add similar Api properties for each new service controller

        /// <summary>Inerface to CryptoIndex Api.</summary>
        public ICryptoIndexApi Api { get; private set; }

        /// <summary>C-tor</summary>
        public CryptoIndexClient(IHttpClientGenerator httpClientGenerator)
        {
            Api = httpClientGenerator.Generate<ICryptoIndexApi>();
        }
    }
}
