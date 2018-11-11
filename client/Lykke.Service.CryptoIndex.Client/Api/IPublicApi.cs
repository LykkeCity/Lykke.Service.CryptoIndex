using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.CryptoIndex.Client.Models;
using Refit;

namespace Lykke.Service.CryptoIndex.Client.Api
{
    /// <summary>
    /// Api for lykke.com
    /// </summary>
    [PublicAPI]
    public interface IPublicApi
    {
        /// <summary>
        /// Returns indices
        /// </summary>
        [Get("/api/public/indices")]
        [Obsolete]
        Task<IReadOnlyList<(DateTime, decimal)>> GetIndexHistoriesAsync(DateTime from, DateTime to);

        /// <summary>
        /// Returns indices up to date
        /// </summary>
        [Get("/api/public/indices/upToDate")]
        Task<IReadOnlyList<(DateTime, decimal)>> GetIndexHistoriesAsync(DateTime to, int limit);

        /// <summary>
        /// Returns current index value
        /// </summary>
        [Get("/api/public/index/current")]
        [Obsolete]
        Task<(DateTime, decimal)> GetCurrentAsync();

        /// <summary>
        /// Returns last index
        /// </summary>
        [Get("/api/public/index/last")]
        Task<PublicIndexHistory> GetLastAsync();

        /// <summary>
        /// Returns value at today midnight and last value
        /// </summary>
        [Get("/api/public/change")]
        Task<IReadOnlyList<(DateTime, decimal)>> GetChangeAsync();
    }
}
