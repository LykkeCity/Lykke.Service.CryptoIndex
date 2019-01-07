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
        [Obsolete("Use GetKeyNumbers().CurrentValue instead.")]
        Task<PublicIndexHistory> GetLastAsync();

        /// <summary>
        /// Returns value at today midnight and last value
        /// </summary>
        [Get("/api/public/change")]
        [Obsolete("Use GetKeyNumbers().Return24H instead.")]
        Task<IReadOnlyList<(DateTime, decimal)>> GetChangeAsync();

        /// <summary>
        /// Returns chart data by time interval
        /// </summary>
        [Get("/api/public/indexHistory/{timeInterval}")]
        Task<IDictionary<DateTime, decimal>> GetIndexHistory(TimeInterval timeInterval);

        /// <summary>
        /// Returns key numbers
        /// </summary>
        [Get("/api/public/keyNumbers")]
        Task<KeyNumbers> GetKeyNumbers();
    }
}
