using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Common;
using Lykke.AzureStorage.Tables;
using Lykke.Service.CryptoIndex.Domain.LCI10.IndexHistory;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.CryptoIndex.Domain.AzureRepositories.LCI10.IndexHistory
{
    public class IndexHistoryRepository : IIndexHistoryRepository
    {
        private readonly INoSQLTableStorage<IndexHistoryEntity> _storage;

        public IndexHistoryRepository(INoSQLTableStorage<IndexHistoryEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IReadOnlyList<Domain.LCI10.IndexHistory.IndexHistory>> GetAsync(DateTime from, DateTime to)
        {
            var pKeyFrom = GetPartitionKey(from);
            var pKeyTo = GetPartitionKey(to);
            var rowKeyFrom = GetRowKey(from);
            var rowKeyTo = GetRowKey(to);

            var query = new TableQuery<IndexHistoryEntity>();

            var pKeyCondFrom = TableQuery.GenerateFilterCondition(nameof(AzureTableEntity.PartitionKey), QueryComparisons.GreaterThanOrEqual, pKeyFrom);
            var pKeyCondTo = TableQuery.GenerateFilterCondition(nameof(AzureTableEntity.PartitionKey), QueryComparisons.LessThanOrEqual, pKeyTo);
            var pKeyFilter = TableQuery.CombineFilters(pKeyCondFrom, TableOperators.And, pKeyCondTo);

            var rowKeyCondFrom = TableQuery.GenerateFilterCondition(nameof(AzureTableEntity.RowKey), QueryComparisons.GreaterThanOrEqual, rowKeyFrom);
            var rowKeyCondTo = TableQuery.GenerateFilterCondition(nameof(AzureTableEntity.RowKey), QueryComparisons.LessThanOrEqual, rowKeyTo);
            var rowKeyFilter = TableQuery.CombineFilters(rowKeyCondFrom, TableOperators.And, rowKeyCondTo);

            query.FilterString = TableQuery.CombineFilters(pKeyFilter, TableOperators.And, rowKeyFilter);

            var model = await _storage.WhereAsync(query);

            var domain = Mapper.Map<IReadOnlyList<Domain.LCI10.IndexHistory.IndexHistory>>(model);

            return domain;
        }

        public async Task InsertAsync(Domain.LCI10.IndexHistory.IndexHistory indexHistory)
        {
            var model = Mapper.Map<IndexHistoryEntity>(indexHistory);
            model.PartitionKey = GetPartitionKey(indexHistory.Time);
            model.RowKey = GetRowKey(indexHistory.Time);

            await _storage.InsertOrReplaceAsync(model);
        }

        private static string GetPartitionKey(DateTime time)
            => time.Date.ToIsoDate();

        private static string GetRowKey(DateTime time)
            => time.ToIsoDateTime();
    }
}
