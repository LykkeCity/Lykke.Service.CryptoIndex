using System;
using Newtonsoft.Json;

namespace Lykke.Service.CryptoIndex.RabbitMq.Models
{
    internal sealed class TickPrice
    {
        [JsonProperty("source")]
        public string Source { get; }

        [JsonProperty("asset")]
        public string AssetPair { get; }

        [JsonProperty("bid")]
        public decimal? Bid { get; }

        [JsonProperty("ask")]
        public decimal? Ask { get; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; }
    }
}
