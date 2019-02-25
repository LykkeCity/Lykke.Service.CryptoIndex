using System;

namespace Lykke.Service.CryptoIndex.Domain.Models
{
    /// <summary>
    /// Tick price (ticker item)
    /// </summary>
    public class TickPrice
    {
        /// <summary>
        /// Source, an exchange name usually
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Asset pair
        /// </summary>
        public string AssetPair { get; }

        /// <summary>
        /// Best bid price
        /// </summary>
        public decimal? Bid { get; }

        /// <summary>
        /// Best ask price
        /// </summary>
        public decimal? Ask { get; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Timestamp { get; }

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
        public TickPrice(string source, string assetPair, decimal? bid, decimal? ask, DateTime timestamp)
        {
            Source = source;
            AssetPair = assetPair;
            Bid = bid;
            Ask = ask;
            Timestamp = timestamp;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Source}, {AssetPair}, bid={Bid}, ask={Ask}, mid={MiddlePrice}, {Timestamp}";
        }
    }
}
