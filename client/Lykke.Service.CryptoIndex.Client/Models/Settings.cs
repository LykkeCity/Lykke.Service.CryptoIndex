﻿using System.Collections.Generic;
using JetBrains.Annotations;

namespace Lykke.Service.CryptoIndex.Client.Models
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
        /// White list of assets
        /// </summary>
        public IReadOnlyList<string> Assets { get; set; } = new List<string>();

        /// <summary>
        /// Count of the top assets
        /// </summary>
        public int TopCount { get; set; }

        /// <summary>
        /// Is crypto index calculation enabled
        /// </summary>
        public bool Enabled { get; set; }
    }
}