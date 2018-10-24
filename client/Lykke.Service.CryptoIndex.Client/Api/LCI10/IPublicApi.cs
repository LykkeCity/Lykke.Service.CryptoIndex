using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.CryptoIndex.Client.Models.LCI10;
using Refit;

namespace Lykke.Service.CryptoIndex.Client.Api.LCI10
{
    /// <summary>
    /// Api for lykke.com
    /// </summary>
    [PublicAPI]
    public interface IPublicApi
    {
        /// <summary>
        /// Returns current and previous tick prices
        /// </summary>
        [Get("/api/public/twoTickPrices")]
        Task<TwoTickPrices> GetTwoTickPricesAsync();
    }
}
