namespace Lykke.Service.CryptoIndex.Domain.Repositories.Models
{
    /// <summary>
    /// Represents an asset price, including cross asset prices like ADA/USD = ADA/BTC * BTC/USD
    /// </summary>
    public class AssetPriceEntity
    {
        /// <summary>
        /// Asset, like ADA
        /// </summary>
        public string Asset { get; set; }

        /// <summary>
        /// Cross asset, like BTC or ETH
        /// </summary>
        public string CrossAsset { get; set; }

        /// <summary>
        /// Source
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Price in USD
        /// </summary>
        public decimal Price { get; set; }
    }
}
