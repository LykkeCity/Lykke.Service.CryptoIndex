namespace Lykke.Service.CryptoIndex.Domain.AzureRepositories.MarketCap
{
    public class AssetMarketCapEntity
    {
        public string Asset { get; set; }

        public MarketCapEntity MarketCap { get; set; }
    }
}
