using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Common;
using Lykke.AzureStorage.Tables;
using Lykke.Service.CryptoIndex.Domain.Models;
using Lykke.Service.CryptoIndex.Domain.Repositories.Models;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.CryptoIndex.Domain.Repositories.Repositories
{
    public class WarningRepository : IWarningRepository
    {
        private readonly INoSQLTableStorage<WarningEntity> _storage;

        public WarningRepository(INoSQLTableStorage<WarningEntity> storage)
        {
            _storage = storage;
        }

        public async Task SaveAsync(Warning warning)
        {
            var model = Mapper.Map<WarningEntity>(warning);
            model.PartitionKey = GetPartitionKey(warning.Time);
            model.RowKey = GetRowKey(warning.Time);

            await _storage.InsertAsync(model);
        }
        
        public async Task<IReadOnlyList<Warning>> GetAsync(DateTime from, DateTime to)
        {
            var pKeyFrom = GetPartitionKey(from);
            var pKeyTo = GetPartitionKey(to);
            var rowKeyFrom = GetRowKey(from);
            var rowKeyTo = GetRowKey(to);

            var query = new TableQuery<WarningEntity>();

            var pKeyCondFrom = TableQuery.GenerateFilterCondition(nameof(AzureTableEntity.PartitionKey), QueryComparisons.GreaterThanOrEqual, pKeyFrom);
            var pKeyCondTo = TableQuery.GenerateFilterCondition(nameof(AzureTableEntity.PartitionKey), QueryComparisons.LessThanOrEqual, pKeyTo);
            var pKeyFilter = TableQuery.CombineFilters(pKeyCondFrom, TableOperators.And, pKeyCondTo);

            var rowKeyCondFrom = TableQuery.GenerateFilterCondition(nameof(AzureTableEntity.RowKey), QueryComparisons.GreaterThanOrEqual, rowKeyFrom);
            var rowKeyCondTo = TableQuery.GenerateFilterCondition(nameof(AzureTableEntity.RowKey), QueryComparisons.LessThanOrEqual, rowKeyTo);
            var rowKeyFilter = TableQuery.CombineFilters(rowKeyCondFrom, TableOperators.And, rowKeyCondTo);

            query.FilterString = TableQuery.CombineFilters(pKeyFilter, TableOperators.And, rowKeyFilter);

            var models = await _storage.WhereAsync(query);

            var domain = Mapper.Map<IReadOnlyList<Warning>>(models);
            
            return domain;
        }

        private static string GetPartitionKey(DateTime time)
            => time.Date.ToIsoDate();

        private static string GetRowKey(DateTime time)
            => time.ToIsoDateTime();
    }
}
