using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.CryptoIndex.Client.Models.LCI10;
using Refit;

namespace Lykke.Service.CryptoIndex.Client.Api.LCI10
{
    /// <summary>
    /// Provides methods to work with index history
    /// </summary>
    [PublicAPI]
    public interface IIndexHistoryApi
    {
        /// <summary>
        /// Returns index history
        /// </summary>
        [Get("/api/indexHistory/indexHistories")]
        Task<IReadOnlyList<IndexHistory>> GetIndexHistoriesAsync(DateTime from, DateTime to);

        /// <summary>
        /// Returns timestamps
        /// </summary>
        [Get("/api/indexHistory/timestamps")]
        Task<IReadOnlyList<DateTime>> GetTimestampsAsync(DateTime from, DateTime to);

        /// <summary>
        /// Returns an index history element
        /// </summary>
        [Get("/api/indexHistory")]
        Task<IndexHistory> GetAsync(DateTime timestamp);
    }
}
