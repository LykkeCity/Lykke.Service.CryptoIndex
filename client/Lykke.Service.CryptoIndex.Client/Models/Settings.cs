using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Lykke.Service.CryptoIndex.Client.Models
{
    /// <summary>
    /// Crypto Index settings
    /// </summary>
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class Settings
    {
        /// <summary>
        /// Where the prices shall be taken from
        /// </summary>
        public IReadOnlyList<string> Sources { get; set; } = new List<string>();

        /// <summary>
        /// White list of assets
        /// </summary>
        public IReadOnlyList<string> Assets { get; set; } = new List<string>();

        /// <summary>
        /// List of ignored assets
        /// </summary>
        public IReadOnlyList<IgnoredAsset> IgnoredAssets { get; set; } = new List<IgnoredAsset>();

        /// <summary>
        /// List of frozen assets
        /// </summary>
        public IReadOnlyList<AssetSettings> AssetsSettings { get; set; } = new List<AssetSettings>();

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

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Sources?.Count}, {Assets?.Count}, {AssetsSettings?.Count}, {TopCount}, enabled={Enabled}, {RebuildTime}, {AutoFreezeChangePercents}%, {CrossAssets?.Count}";
        }
    }
}
