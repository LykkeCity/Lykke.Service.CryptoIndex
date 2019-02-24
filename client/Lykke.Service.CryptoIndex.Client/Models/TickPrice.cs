using System;

namespace Lykke.Service.CryptoIndex.Client.Models
{
    /// <summary>
    /// Represents a tick price
    /// </summary>
    public class TickPrice
    {
        /// <summary>
        /// Source, an exchange name usually
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Asset Pair
        /// </summary>
        public string AssetPair { get; set; }

        /// <summary>
        /// Best bid price
        /// </summary>
        public decimal? Bid { get; set; }

        /// <summary>
        /// Best ask price
        /// </summary>
        public decimal? Ask { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Middle price
        /// </summary>
        public decimal? MiddlePrice
        {
            get
            {
                if (Ask.HasValue && !Bid.HasValue)
                    return Ask.Value;

                if (!Ask.HasValue && Bid.HasValue)
                    return Bid.Value;

                if (Ask.HasValue && Bid.HasValue)
                    return (Ask.Value + Bid.Value) / 2;

                return null;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Source}, {AssetPair}, bid={Bid}, ask={Ask}, mid={MiddlePrice}, {Timestamp}";
        }
    }
}
