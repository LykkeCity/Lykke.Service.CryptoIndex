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
        [Get("/api/lci10/indexHistory")]
        Task<IReadOnlyList<IndexHistory>> GetIndexHistoryAsync(DateTime from, DateTime to);
    }
}
