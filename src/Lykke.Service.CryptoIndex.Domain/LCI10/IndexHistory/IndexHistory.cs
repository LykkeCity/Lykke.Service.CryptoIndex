using System;
using System.Collections.Generic;
using Lykke.Service.CryptoIndex.Domain.MarketCapitalization;

namespace Lykke.Service.CryptoIndex.Domain.LCI10.IndexHistory
{
    public class IndexHistory
    {
        public decimal Value { get; }

        public IReadOnlyList<AssetMarketCap> MarketCaps { get; }

        public IDictionary<string, decimal> Weights { get; }

        public IDictionary<string, decimal> MiddlePrices { get; }

        public DateTime Time { get; }

        public IndexHistory(decimal value, IReadOnlyList<AssetMarketCap> marketCaps, IDictionary<string, decimal> weights,
            IDictionary<string, decimal> middlePrices, DateTime time)
        {
            Value = value == default(decimal) ? throw new ArgumentOutOfRangeException(nameof(value)) : value;
            MarketCaps = marketCaps ?? throw new ArgumentNullException(nameof(marketCaps));
            Weights = weights ?? throw new ArgumentNullException(nameof(weights));
            MiddlePrices = middlePrices ?? throw new ArgumentNullException(nameof(middlePrices));
            Time = time == default(DateTime) ? throw new ArgumentOutOfRangeException(nameof(time)) : time;
        }

        public override string ToString()
        {
            return $"{Value}, {Time}";
        }
    }
}
