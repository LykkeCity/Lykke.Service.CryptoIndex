using System;
using System.Collections.Generic;
using Lykke.AzureStorage.Tables;

namespace Lykke.Service.CryptoIndex.Domain.AzureRepositories.LCI10
{
    public class IndexSnapshotEntity : AzureTableEntity
    {
        public decimal Value { get; set; }

        private IReadOnlyList<AssetMarketCapEntity> MarketCaps { get; set; }

        public IDictionary<string, decimal> Weights { get; set; }

        public IDictionary<string, IDictionary<string, decimal>> Prices { get; set; }

        public DateTimeOffset Time { get; set; }

        public IndexSnapshotEntity()
        {
        }

        public IndexSnapshotEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }
    }
}
