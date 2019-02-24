namespace Lykke.Service.CryptoIndex.Domain.Repositories.Models
{
    /// <summary>
    /// Asset market capitalization
    /// </summary>
    public class AssetMarketCapEntity
    {
        /// <summary>
        /// Asset name
        /// </summary>
        public string Asset { get; set; }

        /// <summary>
        /// Market capitalization
        /// </summary>
        public MarketCapEntity MarketCap { get; set; }

        /// <summary>
        /// Circulating supply
        /// </summary>
        public decimal CirculatingSupply { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Asset}, {MarketCap.Value}, {CirculatingSupply}";
        }
    }
}
