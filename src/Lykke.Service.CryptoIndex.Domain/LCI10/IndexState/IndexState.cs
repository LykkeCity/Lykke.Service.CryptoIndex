using System.Collections.Generic;

namespace Lykke.Service.CryptoIndex.Domain.LCI10.IndexState
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
    }
}
