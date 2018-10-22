using System.Collections.Generic;

namespace Lykke.Service.CryptoIndex.Domain.Models.LCI10
{
    public class Settings
    {
        /// <summary>
        /// Where the prices shall be taken from
        /// </summary>
        public IReadOnlyList<string> Sources { get; }

        /// <summary>
        /// Сryptocurrencies pegged to other currencies (which we are not interested in)
        /// </summary>
        public IReadOnlyList<string> ExcludedAssets { get; }

        /// <summary>
        /// Count of the top assets
        /// </summary>
        public int TopCount { get; }

        /// <summary>
        /// Is crypto index calculation enabled
        /// </summary>
        public bool Enabled { get; }

        public Settings(IReadOnlyList<string> sources, IReadOnlyList<string> excludedAssets, int topCount, bool enabled)
        {
            Sources = sources;
            ExcludedAssets = excludedAssets;
            TopCount = topCount;
            Enabled = enabled;
        }
    }
}
