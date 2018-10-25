using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.CryptoIndex.Client.Models;
using Refit;

namespace Lykke.Service.CryptoIndex.Client.Api
{
    /// <summary>
    /// Provides methods to work with warnings
    /// </summary>
    [PublicAPI]
    public interface IWarningsApi
    {
        /// <summary>
        /// Returns warnings
        /// </summary>
        [Get("/api/warnings")]
        Task<IReadOnlyList<Warning>> GetWarningsAsync(DateTime from, DateTime to);
    }
}
