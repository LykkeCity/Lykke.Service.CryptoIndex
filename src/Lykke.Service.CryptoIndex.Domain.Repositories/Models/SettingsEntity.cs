using System;
using System.Collections.Generic;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;

namespace Lykke.Service.CryptoIndex.Domain.Repositories.Models
{
    /// <summary>
    /// Crypto Index settings
    /// </summary>
    public class SettingsEntity : AzureTableEntity
    {
        /// <summary>
        /// Where the prices shall be taken from
        /// </summary>
        [JsonValueSerializer]
        public IReadOnlyList<string> Sources { get; set; }

        /// <summary>
        /// White list of assets
        /// </summary>
        [JsonValueSerializer]
        public IReadOnlyList<string> Assets { get; set; }

        /// <summary>
        /// List of frozen assets
        /// </summary>
        [JsonValueSerializer]
        public IReadOnlyList<AssetSettingsEntity> AssetsSettings { get; set; }

        /// <summary>
        /// Count of the top assets
        /// </summary>
        public int TopCount { get; set; }

        /// <summary>
        /// Is crypto index calculation enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// The time when CoinMarketCap data should be refreshed and weights recalculated.
        /// </summary>
        public TimeSpan RebuildTime { get; set; }

        /// <summary>
        /// Percent when asset will become frozen automatically.
        /// </summary>
        public decimal AutoFreezeChangePercents { get; set; }

        /// <summary>
        /// Cross assets
        /// </summary>
        [JsonValueSerializer]
        public IReadOnlyList<string> CrossAssets { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Sources?.Count}, {Assets?.Count}, {AssetsSettings?.Count}, {TopCount}, enabled={Enabled}, {RebuildTime}, {AutoFreezeChangePercents}%, {CrossAssets?.Count}";
        }
    }
}
