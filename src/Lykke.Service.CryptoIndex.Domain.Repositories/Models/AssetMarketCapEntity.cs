namespace Lykke.Service.CryptoIndex.Domain.Repositories.Models
{
    public class AssetMarketCapEntity
    {
        public string Asset { get; set; }

        public MarketCapEntity MarketCap { get; set; }

        public decimal CirculatingSupply { get; set; }
    }
}
