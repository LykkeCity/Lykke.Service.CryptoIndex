namespace Lykke.Service.CryptoIndex.Domain.AzureRepositories.LCI10
{
    public class AssetMarketCapEntity
    {
        public string Asset { get; set; }

        public MarketCapEntity MarketCap { get; set; }
    }
}
