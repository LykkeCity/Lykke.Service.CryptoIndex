using System;
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

        public static DateTime WithoutMilliseconds(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Kind);
        }
    }
}
