using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.CryptoIndex.Client.Models;
using Refit;

namespace Lykke.Service.CryptoIndex.Client.Api
{
    /// <summary>
    /// Provides methods to work with asset.
    /// </summary>
    [PublicAPI]
    public interface ISettingsApi
    {
        /// <summary>
        /// Returns current settings
        /// </summary>
        [Get("/api/settings")]
        Task<Settings> GetAsync();

        /// <summary>
        /// Set settings
        /// </summary>
        [Post("/api/settings")]
        Task SetAsync(Settings settings);

        /// <summary>
        /// Resets all the records in the database
        /// </summary>
        [Get("/api/settings/reset")]
        Task ResetAsync();

        /// <summary>
        /// Rebuild constituents
        /// </summary>
        [Get("/api/settings/rebuild")]
        Task RebuildAsync();

        /// <summary>
        /// Returns asset pair from publishing IndexTickPrice.
        /// </summary>
        /// <returns></returns>
        [Get("/api/settings/indexTickPriceAssetPairName")]
        Task<string> GetIndexTickPriceAssetPairNameAsync();
    }
}
