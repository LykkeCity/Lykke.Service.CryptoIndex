using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.Service.CryptoIndex.Domain.AzureRepositories.MarketCap;

namespace Lykke.Service.CryptoIndex.Domain.AzureRepositories.LCI10.IndexHistory
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class IndexHistoryEntity : AzureTableEntity
    {
        public decimal Value { get; set; }

        [JsonValueSerializer]
        public IList<AssetMarketCapEntity> MarketCaps { get; set; }

        [JsonValueSerializer]
        public IDictionary<string, decimal> Weights { get; set; }

        [JsonValueSerializer]
        public IDictionary<string, decimal> MiddlePrices { get; set; }

        public DateTime Time { get; set; }
    }
}
