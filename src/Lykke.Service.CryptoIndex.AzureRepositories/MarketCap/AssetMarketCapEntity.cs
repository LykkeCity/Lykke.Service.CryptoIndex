﻿namespace Lykke.Service.CryptoIndex.Domain.Repositories.MarketCap
{
    public class AssetMarketCapEntity
    {
        public string Asset { get; set; }

        public MarketCapEntity MarketCap { get; set; }
    }
}
