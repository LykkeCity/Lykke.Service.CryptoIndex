using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.CryptoIndex.Settings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
}
