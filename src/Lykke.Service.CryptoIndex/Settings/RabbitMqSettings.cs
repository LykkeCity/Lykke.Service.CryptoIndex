using System.Collections.Generic;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.CryptoIndex.Settings
{
    public class RabbitMqSettings
    {
        [AmqpCheck]
        public string ConnectionString { get; set; }

        public IEnumerable<string> SubscribingExchanges { get; set; }

        public string PublishingExchange { get; set; }
    }
}
