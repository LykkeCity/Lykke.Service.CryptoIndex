using JetBrains.Annotations;
using Lykke.Service.CryptoIndex.Client.Api;

namespace Lykke.Service.CryptoIndex.Client
{
    /// <summary>
    /// CryptoIndex client interface.
    /// </summary>
    [PublicAPI]
    public interface ICryptoIndexClient
    {
        /// <summary>
        /// Asset info API
        /// </summary>
        IAssetsInfoApi AssetsInfo { get; }

        /// <summary>
        /// Index history API
        /// </summary>
        IIndexHistoryApi IndexHistory { get; }

        /// <summary>
        /// Public API for lykke.com
        /// </summary>
        IPublicApi Public { get; }

        /// <summary>
        /// Settings API
        /// </summary>
        ISettingsApi Settings { get; }

        /// <summary>
        /// Tick prices API
        /// </summary>
        ITickPricesApi TickPrices { get; }

        /// <summary>
        /// Warnings API
        /// </summary>
        IWarningsApi Warnings { get; }
    }
}
