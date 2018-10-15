using System.Collections.Generic;

namespace Lykke.Service.CryptoIndex.Domain.LCI10.Settings
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

        public Settings(IReadOnlyList<string> sources, IReadOnlyList<string> assets)
        {
            Sources = sources;
            Assets = assets;
        }
    }
}
