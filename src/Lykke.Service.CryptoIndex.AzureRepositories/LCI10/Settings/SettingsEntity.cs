using System.Collections.Generic;
using Lykke.AzureStorage.Tables;

namespace Lykke.Service.CryptoIndex.Domain.AzureRepositories.LCI10.Settings
{
    public class SettingsEntity : AzureTableEntity
    {
        private IReadOnlyList<string> Sources { get; set; }

        private IReadOnlyList<string> Assets { get; set; }

        public SettingsEntity()
        {
        }

        public SettingsEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }
    }
}
