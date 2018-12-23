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
        /// Circulating Supply of asset
        /// </summary>
        public decimal CirculatingSupply { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public AssetMarketCap(string asset, MarketCap marketCap, decimal circulatingSupply)
        {
            if (string.IsNullOrWhiteSpace(asset)) throw new ArgumentOutOfRangeException(nameof(asset));

            Asset = asset;
            MarketCap = marketCap ?? throw new ArgumentNullException(nameof(MarketCap));
            CirculatingSupply = circulatingSupply;
        }
    }
}
