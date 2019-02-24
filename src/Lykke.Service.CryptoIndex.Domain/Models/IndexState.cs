using System;
using System.Collections.Generic;
using System.Linq;

namespace Lykke.Service.CryptoIndex.Domain.Models
{
    /// <summary>
    /// Previous index state, needed for the next index calculation
    /// </summary>
    public class IndexState
    {
        /// <summary>
        /// Index value
        /// </summary>
        public decimal Value { get; }

        /// <summary>
        /// Final used asset prices
        /// </summary>
        public IDictionary<string, decimal> MiddlePrices { get; }

        /// <inheritdoc />
        public IndexState(decimal value, IDictionary<string, decimal> middlePrices)
        {
            Value = value == default(decimal) ? throw new ArgumentOutOfRangeException(nameof(value)) : value;
            MiddlePrices = !middlePrices.Any() ? throw new ArgumentOutOfRangeException(nameof(middlePrices)) : middlePrices;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Value}, {MiddlePrices?.Count}";
        }
    }
}
