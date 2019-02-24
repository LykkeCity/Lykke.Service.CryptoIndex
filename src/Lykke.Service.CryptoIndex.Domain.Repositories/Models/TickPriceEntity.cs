using System;

namespace Lykke.Service.CryptoIndex.Domain.Repositories.Models
{
    public class TickPriceEntity
    {
        public string Source { get; set; }

        public string AssetPair { get; set; }

        public decimal? Bid { get; set; }

        public decimal? Ask { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
