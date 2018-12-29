﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Lykke.AzureStorage.Tables;
using Lykke.Service.CryptoIndex.Domain.Repositories.Models;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.CryptoIndex.Domain.Repositories.Repositories
{
    public abstract class ChartHistoryRepository : IChartHistoryRepository
    {
        private readonly INoSQLTableStorage<HistoryPointEntity> _storage;

        public ChartHistoryRepository(INoSQLTableStorage<HistoryPointEntity> storage)
        {
            _storage = storage;
        }

        public async Task InsertOrReplaceAsync(DateTime time, decimal value)
        {
            var model = new HistoryPointEntity(time, value);
            model.PartitionKey = GetPartitionKey(time);
            model.RowKey = GetRowKey(time);

            await _storage.InsertOrReplaceAsync(model);
        }
        
        public async Task<IReadOnlyDictionary<DateTime, decimal>> GetAsync(DateTime from, DateTime to)
        {
            var filterPk = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition(nameof(AzureTableEntity.PartitionKey), QueryComparisons.GreaterThanOrEqual,
                    GetPartitionKey(from.Date)),
                TableOperators.And,
                TableQuery.GenerateFilterCondition(nameof(AzureTableEntity.PartitionKey), QueryComparisons.LessThanOrEqual,
                    GetPartitionKey(to.Date)));

            var filterRk = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition(nameof(AzureTableEntity.RowKey), QueryComparisons.GreaterThanOrEqual,
                    GetPartitionKey(from)),
                TableOperators.And,
                TableQuery.GenerateFilterCondition(nameof(AzureTableEntity.RowKey), QueryComparisons.LessThanOrEqual,
                    GetPartitionKey(to)));

            var combined = TableQuery.CombineFilters(filterPk, TableOperators.And, filterRk);

            var query = new TableQuery<HistoryPointEntity>().Where(combined);

            var models = await _storage.WhereAsync(query);

            return models.ToDictionary(point => point.Time, point => point.Value);
        }

        private static string GetPartitionKey(DateTime time)
            => time.Date.ToIsoDate();

        private static string GetRowKey(DateTime time)
            => time.ToIsoDateTime();
    }
}
