using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.CryptoIndex.Domain.LCI10.IndexSnapshot;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.CryptoIndex.Domain.AzureRepositories.LCI10.IndexSnapshot
{
    public class IndexSnapshotRepository : IIndexSnapshotRepository
    {
        private readonly INoSQLTableStorage<IndexSnapshotEntity> _storage;

        public IndexSnapshotRepository(INoSQLTableStorage<IndexSnapshotEntity> storage)
        {
            _storage = storage;
        }

        public async Task<Domain.LCI10.IndexSnapshot.IndexSnapshot> GetLatestAsync()
        {
            var query = new TableQuery<IndexSnapshotEntity>().Take(1);

            var model = (await _storage.WhereAsync(query)).FirstOrDefault();

            if (model == null)
                return null;

            var domain = Mapper.Map<Domain.LCI10.IndexSnapshot.IndexSnapshot>(model);

            return domain;
        }

        public async Task InsertAsync(Domain.LCI10.IndexSnapshot.IndexSnapshot indexSnapshot)
        {
            var model = Mapper.Map<IndexSnapshotEntity>(indexSnapshot);
            model.PartitionKey = IndexSnapshotEntity.GeneratePartitionKey(indexSnapshot.Time.Date);
            model.RowKey = IndexSnapshotEntity.GenerateRowKey(indexSnapshot.Time);

            await _storage.InsertOrReplaceAsync(model);
        }
    }
}
