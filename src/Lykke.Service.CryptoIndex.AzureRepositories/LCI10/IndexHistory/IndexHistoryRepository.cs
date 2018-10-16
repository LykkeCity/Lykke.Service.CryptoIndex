using System;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.CryptoIndex.Domain.LCI10.IndexHistory;

namespace Lykke.Service.CryptoIndex.Domain.AzureRepositories.LCI10.IndexHistory
{
    public class IndexHistoryRepository : IIndexHistoryRepository
    {
        private readonly INoSQLTableStorage<IndexHistoryEntity> _storage;

        public IndexHistoryRepository(INoSQLTableStorage<IndexHistoryEntity> storage)
        {
            _storage = storage;
        }

        public async Task InsertAsync(Domain.LCI10.IndexHistory.IndexHistory indexHistory)
        {
            var model = Mapper.Map<IndexHistoryEntity>(indexHistory);
            model.PartitionKey = GetPartitionKey(indexHistory.Time);
            model.RowKey = GetRowKey(indexHistory.Time);

            await _storage.InsertOrReplaceAsync(model);
        }

        private static string GetPartitionKey(DateTime time)
            => time.Date.ToString("yyyy-MM-dd");

        private static string GetRowKey(DateTime time)
            => time.ToString("O");
    }
}
