using System;
using System.Collections.Generic;
using Lykke.AzureStorage.Tables;
using Lykke.Service.CryptoIndex.Domain.MarketCapitalization;

namespace Lykke.Service.CryptoIndex.Domain.AzureRepositories.LCI10
{
    public class SettingsEntity : AzureTableEntity
    {
        private IReadOnlyList<AssetMarketCap> MarketCaps { get; set; }

        private IReadOnlyList<string> Sources { get; set; }

        private IReadOnlyList<string> Assets { get; set; }

        private TimeSpan WeigthsCalculationInterval { get; set; }

        private TimeSpan IndexCalculationInterval { get; set; }

        public SettingsEntity()
        {
        }

        public SettingsEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }
    }
}
