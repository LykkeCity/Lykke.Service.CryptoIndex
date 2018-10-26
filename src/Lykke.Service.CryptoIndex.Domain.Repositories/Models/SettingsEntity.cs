using System.Collections.Generic;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;

namespace Lykke.Service.CryptoIndex.Domain.Repositories.Models
{
    public class SettingsEntity : AzureTableEntity
    {
        [JsonValueSerializer]
        public IReadOnlyList<string> Sources { get; set; }

        [JsonValueSerializer]
        public IReadOnlyList<string> Assets { get; set; }
        
        public int TopCount { get; set; }

        public bool Enabled { get; set; }
    }
}
