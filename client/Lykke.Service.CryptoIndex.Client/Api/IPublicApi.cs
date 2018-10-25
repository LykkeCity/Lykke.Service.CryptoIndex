using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
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
        Task<IReadOnlyList<(DateTime, decimal)>> GetIndexHistoriesAsync(DateTime from, DateTime to);

        /// <summary>
        /// Returns indices up to date
        /// </summary>
        [Get("/api/public/indices/upToDate")]
        Task<IReadOnlyList<(DateTime, decimal)>> GetIndexHistoriesAsync(DateTime to, int limit);

        /// <summary>
        /// Returns current index value
        /// </summary>
        /// <returns></returns>
        [Get("/api/public/index/current")]
        Task<(DateTime, decimal)> GetCurrentAsync();
    }
}
