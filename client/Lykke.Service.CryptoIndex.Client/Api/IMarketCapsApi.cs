using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Refit;

namespace Lykke.Service.CryptoIndex.Client.Api
{
    /// <summary>
    /// Provides methods to work with market capitalization
    /// </summary>
    [PublicAPI]
    public interface IMarketCapsApi
    {
        /// <summary>
        /// Returns market caps assets
        /// </summary>
        [Get("/api/marketCaps/assets")]
        Task<IReadOnlyList<string>> GetAssetsAsync();
    }
}
