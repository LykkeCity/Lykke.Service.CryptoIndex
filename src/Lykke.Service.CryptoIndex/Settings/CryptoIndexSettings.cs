using System;
using JetBrains.Annotations;

namespace Lykke.Service.CryptoIndex.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class CryptoIndexSettings
    {
        public string CoinMarketCapApiKey { get; set; }

        public string IndexName { get; set; }

        public string ShortIndexName { get; set; }

        public bool IsShortIndexEnabled { get; set; }

        public TimeSpan IndexCalculationInterval { get; set; }

        public DbSettings Db { get; set; }

        public RabbitMqSettings RabbitMq { get; set; }
    }
}
