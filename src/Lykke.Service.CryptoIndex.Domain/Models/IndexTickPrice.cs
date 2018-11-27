using System;
using System.Collections.Generic;

namespace Lykke.Service.CryptoIndex.Domain.Models
{
    public class IndexTickPrice
    {
        public string Source { get; }

        public string AssetPair { get; }

        public decimal? Bid { get; }

        public decimal? Ask { get; }

        public DateTime Timestamp { get; }

        public IReadOnlyCollection<AssetInfo> AssetsInfo { get; }

        public IndexTickPrice(string source, string assetPair, decimal? bid, decimal? ask, DateTime timestamp, IReadOnlyCollection<AssetInfo> assetsInfo)
        {
            Source = source;
            AssetPair = assetPair;
            Bid = bid;
            Ask = ask;
            Timestamp = timestamp;
            AssetsInfo = assetsInfo;
        }
    }
}
