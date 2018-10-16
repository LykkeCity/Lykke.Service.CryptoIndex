using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
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

        public async Task<Domain.LCI10.IndexHistory.IndexHistory> GetLatestAsync()
        {
            var query = new TableQuery<IndexHistoryEntity>().Take(1);

            var model = await _storage.WhereAsync(query);

            var domain = Mapper.Map<List<Domain.LCI10.IndexHistory.IndexHistory>>(model).FirstOrDefault();

            return domain;
        }

        public async Task<IReadOnlyList<Domain.LCI10.IndexHistory.IndexHistory>> GetAsync(DateTime from, DateTime to, int limit)
        {
            var filter = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition(nameof(AzureTableEntity.PartitionKey), QueryComparisons.GreaterThan,
                    GetPartitionKey(to.Date.AddDays(1))),
                TableOperators.And,
                TableQuery.GenerateFilterCondition(nameof(AzureTableEntity.PartitionKey), QueryComparisons.LessThan,
                    GetPartitionKey(from.Date.AddMilliseconds(-1))));

            var query = new TableQuery<IndexHistoryEntity>().Where(filter).Take(limit);

            var model = await _storage.WhereAsync(query);

            var domain = Mapper.Map<List<Domain.LCI10.IndexHistory.IndexHistory>>(model);

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
            => (DateTime.MaxValue.Ticks - time.Date.Ticks).ToString("D19");

        private static string GetRowKey(DateTime time)
            => time.ToString("O");
    }
}
