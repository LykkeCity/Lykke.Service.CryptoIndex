using System;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.CryptoIndex.Domain.Repositories.Models;

namespace Lykke.Service.CryptoIndex.Domain.Repositories.Repositories
{
    public class FirstStateAfterResetTimeRepository : IFirstStateAfterResetTimeRepository
    {
        private const string ConstKey = "FirstStateAfterResetTime";
        private readonly INoSQLTableStorage<FirstStateAfterResetTimeEntity> _storage;

        public FirstStateAfterResetTimeRepository(INoSQLTableStorage<FirstStateAfterResetTimeEntity> storage)
        {
            _storage = storage;
        }

        public async Task SetAsync(DateTime dateTime)
        {
            var model = new FirstStateAfterResetTimeEntity { Time = dateTime };
            model.PartitionKey = ConstKey;
            model.RowKey = ConstKey;

            await _storage.InsertOrReplaceAsync(model);
        }

        public async Task<DateTime?> GetAsync()
        {
            var model = await _storage.GetDataAsync(ConstKey, ConstKey);

            return model?.Time;
        }
    }
}
