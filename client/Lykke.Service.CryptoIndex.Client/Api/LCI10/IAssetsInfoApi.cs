using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.CryptoIndex.Client.Models.LCI10;
using Refit;

namespace Lykke.Service.CryptoIndex.Client.Api.LCI10
{
    /// <summary>
    /// Provides methods to work with asset
    /// </summary>
    [PublicAPI]
    public interface IAssetsInfoApi
    {
        /// <summary>
        /// Returns information about asset
        /// </summary>
        [Get("/api/assetsInfo/all")]
        Task<IReadOnlyList<AssetInfo>> GetAllAsync();
    }
}
