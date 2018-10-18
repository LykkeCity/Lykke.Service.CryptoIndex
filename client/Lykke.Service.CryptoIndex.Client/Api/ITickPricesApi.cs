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
        /// Returns information about asset
        /// </summary>
        [Get("/api/tickPrices/sources")]
        Task<IReadOnlyList<string>> GetExchangesAsync();
    }
}
