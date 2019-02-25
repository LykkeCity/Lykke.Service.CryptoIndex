using System;

namespace Lykke.Service.CryptoIndex.Client.Models
{
    /// <summary>
    /// Asset market capitalization
    /// </summary>
    public class AssetMarketCap
    {
        /// <summary>
        /// Asset name
        /// </summary>
        public string Asset { get; }

        /// <summary>
        /// Market capitalization
        /// </summary>
        public MarketCap MarketCap { get; }

        /// <summary>
        /// Circulating supply
        /// </summary>
        public decimal CirculatingSupply { get; }

        /// <inheritdoc />
        public AssetMarketCap(string asset, MarketCap marketCap, decimal circulatingSupply)
        {
            if (string.IsNullOrWhiteSpace(asset)) throw new ArgumentOutOfRangeException(nameof(asset));

            Asset = asset;
            MarketCap = marketCap ?? throw new ArgumentNullException(nameof(MarketCap));
            CirculatingSupply = circulatingSupply;
        }

        ///<inheritdoc />
        public override string ToString()
        {
            return $"{Asset}, {MarketCap.Value}, {CirculatingSupply}";
        }
    }
}
