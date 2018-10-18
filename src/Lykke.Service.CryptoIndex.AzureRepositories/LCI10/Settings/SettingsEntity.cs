using System.Collections.Generic;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;

namespace Lykke.Service.CryptoIndex.Domain.Repositories.LCI10.Settings
{
    public class SettingsEntity : AzureTableEntity
    {
        [JsonValueSerializer]
        public IReadOnlyList<string> Sources { get; set; }

        [JsonValueSerializer]
        public IReadOnlyList<string> Assets { get; set; }
    }
}
