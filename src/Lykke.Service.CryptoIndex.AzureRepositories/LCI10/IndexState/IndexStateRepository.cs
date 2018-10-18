using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.CryptoIndex.Domain.LCI10.IndexState;

namespace Lykke.Service.CryptoIndex.Domain.Repositories.LCI10.IndexState
{
    public class IndexStateRepository : IIndexStateRepository
    {
        private const string ConstKey = "IndexState";
        private readonly INoSQLTableStorage<IndexStateEntity> _storage;

        public IndexStateRepository(INoSQLTableStorage<IndexStateEntity> storage)
        {
            _storage = storage;
        }

        public async Task SetAsync(Domain.LCI10.IndexState.IndexState indexState)
        {
            var model = Mapper.Map<IndexStateEntity>(indexState);
            model.PartitionKey = ConstKey;
            model.RowKey = ConstKey;

            await _storage.InsertOrReplaceAsync(model);
        }

        public async Task<Domain.LCI10.IndexState.IndexState> GetAsync()
        {
            var model = await _storage.GetDataAsync(ConstKey, ConstKey);

            var domain = Mapper.Map<Domain.LCI10.IndexState.IndexState>(model);

            return domain;
        }
    }
}
