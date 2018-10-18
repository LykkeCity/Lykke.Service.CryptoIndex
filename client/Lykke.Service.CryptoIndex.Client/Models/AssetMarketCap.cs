using System;

namespace Lykke.Service.CryptoIndex.Client.Models
{
    /// <summary>
    /// Market Cap information of asset
    /// </summary>
    public class AssetMarketCap
    {
        /// <summary>
        /// Asset
        /// </summary>
        public string Asset { get; }

        /// <summary>
        /// Market Cap
        /// </summary>
        public MarketCap MarketCap { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public AssetMarketCap(string asset, MarketCap marketCap)
        {
            if (string.IsNullOrWhiteSpace(asset)) throw new ArgumentOutOfRangeException(nameof(asset));

            Asset = asset;
            MarketCap = marketCap ?? throw new ArgumentNullException(nameof(MarketCap));
        }
    }
}
