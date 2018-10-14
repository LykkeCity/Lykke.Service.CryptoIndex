using System.Collections.Generic;
using System.Linq;

namespace Lykke.Service.CryptoIndex.Domain
{
    public static class Extentions
    {
        public static IDictionary<string, decimal> Clone(this IDictionary<string, decimal> dictionary)
        {
            return dictionary.ToDictionary(x => x.Key, x => x.Value);
        }

        public static IDictionary<string, IDictionary<string, decimal>> Clone(this IDictionary<string, IDictionary<string, decimal>> value)
        {
            var result = new Dictionary<string, IDictionary<string, decimal>>();

            foreach (var assetPrices in value)
            {
                var exchangesPrices = assetPrices.Value.ToDictionary(x => x.Key, x => x.Value);

                result.Add(assetPrices.Key, exchangesPrices);
            }

            return result;
        }
    }
}
