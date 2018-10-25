using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Refit;

namespace Lykke.Service.CryptoIndex.Client.Api
{
    /// <summary>
    /// Provides methods to work with asset
    /// </summary>
    [PublicAPI]
    public interface ITickPricesApi
    {
        /// <summary>
        /// Returns all available sources
        /// </summary>
        [Get("/api/tickPrices/sources")]
        Task<IReadOnlyList<string>> GetSourcesAsync();

        /// <summary>
        /// Returns all available assets
        /// </summary>
        [Get("/api/tickPrices/assets")]
        Task<IReadOnlyList<string>> GetAssetsAsync();
    }
}
