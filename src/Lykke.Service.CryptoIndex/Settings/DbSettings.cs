using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.CryptoIndex.Settings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnectionString { get; set; }

        [AzureTableCheck]
        public string DataConnectionString { get; set; }
    }
}
