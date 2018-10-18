using System;

namespace Lykke.Service.CryptoIndex.Domain.Models
{
    public class AssetMarketCap
    {
        public string Asset { get; }

        public MarketCap MarketCap { get; }

        public AssetMarketCap(string asset, MarketCap marketCap)
        {
            if (string.IsNullOrWhiteSpace(asset)) throw new ArgumentOutOfRangeException(nameof(asset));

            Asset = asset;
            MarketCap = marketCap ?? throw new ArgumentNullException(nameof(MarketCap));
        }
    }
}
