using System.Collections.Generic;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;

namespace Lykke.Service.CryptoIndex.Domain.Repositories.Models
{
    /// <summary>
    /// Previous index state, needed for the next index calculation
    /// </summary>
    public class IndexStateEntity : AzureTableEntity
    {
        /// <summary>
        /// Index value
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// Final used asset prices
        /// </summary>
        [JsonValueSerializer]
        public IDictionary<string, decimal> MiddlePrices { get; set; }
    }
}
