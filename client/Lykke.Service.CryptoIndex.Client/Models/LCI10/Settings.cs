using System.Collections.Generic;
using JetBrains.Annotations;

namespace Lykke.Service.CryptoIndex.Client.Models.LCI10
{
    /// <summary>
    /// Represents settings information
    /// </summary>
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
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

        /// <summary>
        /// Is crypto index calculation enabled
        /// </summary>
        public bool Enabled { get; set; }
    }
}
