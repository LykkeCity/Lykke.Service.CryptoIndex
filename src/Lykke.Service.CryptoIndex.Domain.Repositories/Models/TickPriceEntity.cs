using System;

namespace Lykke.Service.CryptoIndex.Domain.Repositories.Models
{
    /// <summary>
    /// Tick price (ticker item)
    /// </summary>
    public class TickPriceEntity
    {
        /// <summary>
        /// Source, an exchange name usually
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Asset pair
        /// </summary>
        public string AssetPair { get; set; }

        /// <summary>
        /// Bid
        /// </summary>
        public decimal? Bid { get; set; }

        /// <summary>
        /// Ask
        /// </summary>
        public decimal? Ask { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Source}, {AssetPair}, bid={Bid}, ask={Ask}, {Timestamp}";
        }
    }
}
