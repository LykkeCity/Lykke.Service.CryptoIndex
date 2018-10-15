using System;
using Newtonsoft.Json;

namespace Lykke.Service.CryptoIndex.RabbitMq.Models
{
    internal sealed class TickPrice
    {
        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("asset")]
        public string AssetPair { get; set; }

        [JsonProperty("bid")]
        public decimal? Bid { get; set; }

        [JsonProperty("ask")]
        public decimal? Ask { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
