using JetBrains.Annotations;
using Lykke.Service.CryptoIndex.Client.Api.LCI10;

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
        IAssetInfoApi AssetInfo { get; }

        /// <summary>
        /// Settings API
        /// </summary>
        ISettingsApi Settings { get; }

        /// <summary>
        /// Index history API
        /// </summary>
        IIndexHistoryApi IndexHistory { get; }
    }
}
