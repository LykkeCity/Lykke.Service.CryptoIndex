using System;
using System.Collections.Generic;
using System.Linq;

namespace Lykke.Service.CryptoIndex.Domain.Models
{
    public class IndexState
    {
        public decimal Value { get; }

        public IDictionary<string, decimal> MiddlePrices { get; }

        public IReadOnlyCollection<string> FrozenAssets { get; }

        public IndexState(decimal value, IDictionary<string, decimal> middlePrices, IReadOnlyCollection<string> frozenAssets)
        {
            Value = value == default(decimal) ? throw new ArgumentOutOfRangeException(nameof(value)) : value;
            MiddlePrices = !middlePrices.Any() ? throw new ArgumentOutOfRangeException(nameof(middlePrices)) : middlePrices;
            FrozenAssets = frozenAssets;
        }

        public override string ToString()
        {
            return $"{Value}";
        }
    }
}
