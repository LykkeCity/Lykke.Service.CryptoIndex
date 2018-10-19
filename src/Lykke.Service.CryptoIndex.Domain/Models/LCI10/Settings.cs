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
        /// Constituents of index (assets which we build the index from)
        /// </summary>
        public IReadOnlyList<string> Assets { get; }

        /// <summary>
        /// Is crypto index calculation enabled
        /// </summary>
        public bool Enabled { get; }

        public Settings(IReadOnlyList<string> sources, IReadOnlyList<string> assets, bool enabled)
        {
            Sources = sources;
            Assets = assets;
            Enabled = enabled;
        }
    }
}
