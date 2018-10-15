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
        [Get("/api/lci10/settings")]
        Task<Settings> GetAsync();

        /// <summary>
        /// Set settings
        /// </summary>
        [Post("/api/lci10/settings")]
        Task SetAsync(Settings settings);
    }
}
