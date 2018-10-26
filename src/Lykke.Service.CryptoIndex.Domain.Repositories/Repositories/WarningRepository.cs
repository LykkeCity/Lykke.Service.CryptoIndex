using System;
using System.Collections.Generic;
using System.Linq;
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
            var filter = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition(nameof(AzureTableEntity.PartitionKey), QueryComparisons.GreaterThan,
                    GetPartitionKey(to)),
                TableOperators.And,
                TableQuery.GenerateFilterCondition(nameof(AzureTableEntity.PartitionKey), QueryComparisons.LessThan,
                    GetPartitionKey(from)));

            var query = new TableQuery<WarningEntity>().Where(filter);

            var models = await _storage.WhereAsync(query);

            var domain = Mapper.Map<IReadOnlyList<Warning>>(models);

            domain = domain.OrderBy(x => x.Time).ToList();

            return domain;
        }

        public async Task<IReadOnlyList<Warning>> TakeAsync(int limit)
        {
            var query = new TableQuery<WarningEntity>().Take(limit);

            var models = await _storage.WhereAsync(query);

            var domain = Mapper.Map<IReadOnlyList<Warning>>(models);

            return domain;
        }

        private static string GetPartitionKey(DateTime time)
            => (DateTime.MaxValue.Ticks - time.Ticks).ToString();

        private static string GetRowKey(DateTime time)
            => time.ToIsoDateTime();
    }
}
