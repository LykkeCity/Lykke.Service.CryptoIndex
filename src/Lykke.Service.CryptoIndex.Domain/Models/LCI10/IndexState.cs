using System.Collections.Generic;

namespace Lykke.Service.CryptoIndex.Domain.Models.LCI10
{
    public class IndexState
    {
        public decimal Value { get; }

        public IDictionary<string, decimal> MiddlePrices { get; }

        public IndexState(decimal value, IDictionary<string, decimal> middlePrices)
        {
            Value = value;
            MiddlePrices = middlePrices;
        }

        public override string ToString()
        {
            return $"{Value}";
        }
    }
}
