using System;

namespace Lykke.Service.CryptoIndex.Domain.Models
{
    public class TickPrice
    {
        public string Source { get; }

        public string AssetPair { get; }

        public decimal? Bid { get; }

        public decimal? Ask { get; }

        public DateTime Timestamp { get; }

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

        public TickPrice(string source, string assetPair, decimal? bid, decimal? ask, DateTime timestamp)
        {
            Source = source;
            AssetPair = assetPair;
            Bid = bid;
            Ask = ask;
            Timestamp = timestamp;
        }
    }
}
