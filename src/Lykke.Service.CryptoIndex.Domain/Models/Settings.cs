using System;
using System.Collections.Generic;

namespace Lykke.Service.CryptoIndex.Domain.Models
{
    public class Settings
    {
        /// <summary>
        /// Where the prices shall be taken from
        /// </summary>
        public IReadOnlyList<string> Sources { get; set; }

        /// <summary>
        /// White list of assets
        /// </summary>
        public IReadOnlyList<string> Assets { get; set; }

        /// <summary>
        /// List of frozen assets
        /// </summary>
        public IReadOnlyList<AssetSettings> AssetsSettings { get; set; }

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
        public IReadOnlyList<string> CrossAssets { get; set; }
    }
}
