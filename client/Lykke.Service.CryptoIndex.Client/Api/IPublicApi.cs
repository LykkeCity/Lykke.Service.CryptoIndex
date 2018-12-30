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
        /// Returns last index
        /// </summary>
        [Get("/api/public/index/last")]
        Task<PublicIndexHistory> GetLastAsync();

        /// <summary>
        /// Returns value at today midnight and last value
        /// </summary>
        [Get("/api/public/change")]
        Task<IReadOnlyList<(DateTime, decimal)>> GetChangeAsync();

        /// <summary>
        /// Returns chart data for the last 24 hours
        /// </summary>
        [Get("/api/public/indexHistory24h")]
        Task<IDictionary<DateTime, decimal>> GetIndexHistory24h();

        /// <summary>
        /// Returns chart data for the last 5 days
        /// </summary>
        [Get("/api/public/indexHistory5d")]
        Task<IDictionary<DateTime, decimal>> GetIndexHistory5d();

        /// <summary>
        /// Returns chart data for the last 30 days
        /// </summary>
        [Get("/api/public/indexHistory30d")]
        Task<IDictionary<DateTime, decimal>> GetIndexHistory30d();

        /// <summary>
        /// Returns key numbers
        /// </summary>
        [Get("/api/public/keyNumbers")]
        Task<KeyNumbers> GetKeyNumbers();
    }
}
