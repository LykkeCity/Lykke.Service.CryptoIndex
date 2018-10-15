using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Service.CryptoIndex.Domain.MarketCapitalization;

namespace Lykke.Service.CryptoIndex.Domain.LCI10.IndexSnapshot
{
    public class IndexSnapshot
    {
        public decimal Value { get; }

        public IList<AssetMarketCap> MarketCaps { get; }

        public IDictionary<string, decimal> Weights { get; }

        public IDictionary<string, IDictionary<string, decimal>> Prices { get; }

        public DateTimeOffset Time { get; }

        public IndexSnapshot(decimal value, IList<AssetMarketCap> marketCaps, IDictionary<string, decimal> weights,
            IDictionary<string, IDictionary<string, decimal>> prices, DateTimeOffset time)
        {
            Value = value == default(decimal) ? throw new ArgumentOutOfRangeException(nameof(value)) : value;
            MarketCaps = marketCaps == null || !marketCaps.Any() ? throw new ArgumentOutOfRangeException($"{nameof(marketCaps)} is empty.") : marketCaps;
            Weights = weights ?? throw new ArgumentNullException(nameof(weights));
            Prices = prices ?? throw new ArgumentNullException(nameof(prices));
            Time = time == default(DateTimeOffset) ? throw new ArgumentOutOfRangeException(nameof(time)) : time;
        }
    }
}
