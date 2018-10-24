using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.CryptoIndex.Client.Models.LCI10;
using Refit;

namespace Lykke.Service.CryptoIndex.Client.Api.LCI10
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
        [Get("/api/indexHistory/reset")]
        Task ResetAsync();
    }
}
