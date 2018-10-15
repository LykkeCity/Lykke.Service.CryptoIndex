using System;

namespace Lykke.Service.CryptoIndex.Domain.TickPrice
{
    public class TickPrice
    {
        public string Source { get; }

        public string AssetPair { get; }

        public decimal? Bid { get; }

        public decimal? Ask { get; }

        public DateTime Timestamp { get; }

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
