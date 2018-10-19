using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Common;
using Lykke.AzureStorage.Tables;
using Lykke.Service.CryptoIndex.Domain.Models;
using Lykke.Service.CryptoIndex.Domain.Models.LCI10;
using Lykke.Service.CryptoIndex.Domain.Repositories.LCI10;
using Lykke.Service.CryptoIndex.Domain.Repositories.Models.LCI10;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.CryptoIndex.Domain.Repositories.Repositories.LCI10
{
    public class IndexHistoryRepository : IIndexHistoryRepository
    {
        private readonly INoSQLTableStorage<IndexHistoryEntity> _storage;
        private readonly IndexHistoryBlobRepository _blobRepository;

        public IndexHistoryRepository(INoSQLTableStorage<IndexHistoryEntity> storage, IndexHistoryBlobRepository blobRepository)
        {
            _storage = storage;
            _blobRepository = blobRepository;
        }

        public async Task<IReadOnlyList<IndexHistory>> GetAsync(DateTime from, DateTime to)
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

            var models = await _storage.WhereAsync(query);

            var domain = models.Select(x => new IndexHistory(x.Value, Mapper.Map<IReadOnlyList<AssetMarketCap>>(x.MarketCaps), x.Weights,
                new Dictionary<string, IDictionary<string, decimal>>(), x.MiddlePrices, x.Time)).ToList();
            
            return domain;
        }

        public async Task<IReadOnlyList<DateTime>> GetTimestampsAsync(DateTime from, DateTime to)
        {
            var indexHistories = await GetAsync(from, to);

            var timestamps = indexHistories.Select(x => x.Time).ToList();

            return timestamps;
        }

        public async Task InsertAsync(IndexHistory domain)
        {
            // Table
            var entity = Mapper.Map<IndexHistoryEntity>(domain);
            entity.PartitionKey = GetPartitionKey(domain.Time);
            entity.RowKey = GetRowKey(domain.Time);
            await _storage.InsertOrReplaceAsync(entity);

            // Blob
            var blob = Mapper.Map<IndexHistoryBlob>(domain);
            await _blobRepository.SaveAsync(blob);
        }

        public async Task<IndexHistory> GetAsync(DateTime dateTime)
        {
            var model = await _storage.GetDataAsync(GetPartitionKey(dateTime), GetRowKey(dateTime));

            if (model == null)
                return null;

            var blob = await _blobRepository.GetAsync(dateTime);

            if (blob == null)
                return null;

            var domain = new IndexHistory(model.Value, Mapper.Map<IReadOnlyList<AssetMarketCap>>(model.MarketCaps), model.Weights, 
                blob.Prices, model.MiddlePrices, model.Time);

            return domain;
        }

        public async Task Clear()
        {
            await _storage.DeleteAsync();
            await _storage.CreateTableIfNotExistsAsync();
            await _blobRepository.Clear();
        }

        private static string GetPartitionKey(DateTime time)
            => time.Date.ToIsoDate();

        private static string GetRowKey(DateTime time)
            => time.ToIsoDateTime();
    }
}
