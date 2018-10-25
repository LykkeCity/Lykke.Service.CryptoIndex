using System;
using System.Collections.Generic;
using System.Linq;

namespace Lykke.Service.CryptoIndex.Domain.Models.LCI10
{
    public class IndexState
    {
        public decimal Value { get; }

        public IDictionary<string, decimal> MiddlePrices { get; }

        public IndexState(decimal value, IDictionary<string, decimal> middlePrices)
        {
            Value = value == default(decimal) ? throw new ArgumentOutOfRangeException(nameof(value)) : value;
            MiddlePrices = !middlePrices.Any() ? throw new ArgumentOutOfRangeException(nameof(middlePrices)) : middlePrices;
        }

        public override string ToString()
        {
            return $"{Value}";
        }
    }
}
