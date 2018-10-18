using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.CryptoIndex.Domain.Models;
using Lykke.Service.CryptoIndex.Domain.Models.LCI10;
using Lykke.Service.CryptoIndex.Domain.Repositories.LCI10;
using Lykke.Service.CryptoIndex.Domain.Repositories.Models.LCI10;

namespace Lykke.Service.CryptoIndex.Domain.Repositories.Repositories.LCI10
{
    public class SettingsRepository : ISettingsRepository
    {
        private readonly INoSQLTableStorage<SettingsEntity> _storage;

        public SettingsRepository(INoSQLTableStorage<SettingsEntity> storage)
        {
            _storage = storage;
        }

        public async Task<Settings> GetAsync()
        {
            var settings = await _storage.GetDataAsync(GetPartitionKey(), GetRowKey());
            var domain = Mapper.Map<Settings>(settings);

            return domain;
        }

        public async Task InsertOrReplaceAsync(Settings settings)
        {
            var model = Mapper.Map<SettingsEntity>(settings);
            model.PartitionKey = GetPartitionKey();
            model.RowKey = GetRowKey();

            await _storage.InsertOrReplaceAsync(model);
        }

        private static string GetPartitionKey()
            => "Settings";

        private static string GetRowKey()
            => "Settings";
    }
}
