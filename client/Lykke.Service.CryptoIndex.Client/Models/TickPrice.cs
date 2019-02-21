using System;

namespace Lykke.Service.CryptoIndex.Client.Models
{
    /// <summary>
    /// Represents a tick price
    /// </summary>
    public class TickPrice
    {
        /// <summary>
        /// Source
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Asset Pair
        /// </summary>
        public string AssetPair { get; set; }

        /// <summary>
        /// Bid price
        /// </summary>
        public decimal? Bid { get; set; }

        /// <summary>
        /// Ask price
        /// </summary>
        public decimal? Ask { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
