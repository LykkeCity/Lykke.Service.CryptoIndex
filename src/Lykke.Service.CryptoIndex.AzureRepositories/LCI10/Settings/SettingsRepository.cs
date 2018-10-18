using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.CryptoIndex.Domain.LCI10.Settings;

namespace Lykke.Service.CryptoIndex.Domain.Repositories.LCI10.Settings
{
    public class SettingsRepository : ISettingsRepository
    {
        private readonly INoSQLTableStorage<SettingsEntity> _storage;

        public SettingsRepository(INoSQLTableStorage<SettingsEntity> storage)
        {
            _storage = storage;
        }

        public async Task<Domain.LCI10.Settings.Settings> GetAsync()
        {
            var settings = await _storage.GetDataAsync(GetPartitionKey(), GetRowKey());
            var domain = Mapper.Map<Domain.LCI10.Settings.Settings>(settings);

            return domain;
        }

        public async Task InsertOrReplaceAsync(Domain.LCI10.Settings.Settings settings)
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
