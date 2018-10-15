﻿using System;
using System.Collections.Generic;
using Lykke.AzureStorage.Tables;
using Lykke.Service.CryptoIndex.Domain.AzureRepositories.MarketCap;

namespace Lykke.Service.CryptoIndex.Domain.AzureRepositories.LCI10.IndexSnapshot
{
    public class IndexSnapshotEntity : AzureTableEntity
    {
        public decimal Value { get; set; }

        private IReadOnlyList<AssetMarketCapEntity> MarketCaps { get; set; }

        public IDictionary<string, decimal> Weights { get; set; }

        public IDictionary<string, IDictionary<string, decimal>> Prices { get; set; }

        public DateTimeOffset Time { get; set; }

        public static string GeneratePartitionKey(DateTimeOffset dateTimeOffset)
        {
            return $"{dateTimeOffset:yyyy-MM-dd}";
        }

        public static string GenerateRowKey(DateTimeOffset dateTimeOffset)
        {
            return $"{dateTimeOffset.ToString()}";
        }
    }
}