using System;
using JetBrains.Annotations;

namespace Lykke.Service.CryptoIndex.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class CryptoIndexSettings
    {
        public string CoinMarketCapApiKey { get; set; }

        public TimeSpan WeightsCalculationInterval { get; set; }

        public TimeSpan IndexCalculationInterval { get; set; }

        public DbSettings Db { get; set; }

        public RabbitMqSettings RabbitMq { get; set; }
    }
}
