using System;
using System.Collections.Generic;
using Common;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;

namespace Lykke.Service.CryptoIndex.Domain.Repositories.Models
{
    /// <summary>
    /// Represents a snapshot of one index calculation
    /// </summary>
    public class IndexHistoryEntity : AzureTableEntity
    {
        /// <summary>
        /// Index value
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// Markets capitalization
        /// </summary>
        [JsonValueSerializer]
        public IList<AssetMarketCapEntity> MarketCaps { get; set; }

        /// <summary>
        /// Weights
        /// </summary>
        [JsonValueSerializer]
        public IDictionary<string, decimal> Weights { get; set; }

        /// <summary>
        /// Final used asset prices
        /// </summary>
        [JsonValueSerializer]
        public IDictionary<string, decimal> MiddlePrices { get; set; }

        /// <summary>
        /// Assets settings
        /// </summary>
        [JsonValueSerializer]
        public IReadOnlyList<AssetSettingsEntity> AssetsSettings { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Time { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Time.ToIsoDateTime()}";
        }
    }
}
