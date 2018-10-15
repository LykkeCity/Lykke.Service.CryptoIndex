using System.Collections.Generic;

namespace Lykke.Service.CryptoIndex.Client.Models.LCI10
{
    /// <summary>
    /// Represents settings information
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Where the prices shall be taken from
        /// </summary>
        public IReadOnlyList<string> Sources { get; set; } = new List<string>();

        /// <summary>
        /// Constituents of index (assets which we build the index from)
        /// </summary>
        public IReadOnlyList<string> Assets { get; set; } = new List<string>();
    }
}
