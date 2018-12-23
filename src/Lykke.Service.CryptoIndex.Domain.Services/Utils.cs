using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Service.CryptoIndex.Domain.Models;

namespace Lykke.Service.CryptoIndex.Domain.Services
{
    public static class Utils
    {
        public static decimal GetMiddlePrice(string asset, IReadOnlyCollection<decimal> assetPrices)
        {
            if (string.IsNullOrWhiteSpace(asset))
                throw new ArgumentException("Empty asset.");

            if (assetPrices == null || assetPrices.Count == 0)
                throw new ArgumentOutOfRangeException($"Asset '{asset}' doesn't have any prices.");

            var prices = assetPrices.OrderBy(x => x).ToList();

            if (prices.Count > 2)
            {
                prices.RemoveAt(0);
                prices.RemoveAt(prices.Count - 1);
            }

            var middlePrice = prices.Sum() / prices.Count;

            return middlePrice;
        }

        public static decimal GetPreviousMiddlePrice(string asset, IndexState lastIndex, decimal currentMiddlePrice)
        {
            if (lastIndex == null)
                return currentMiddlePrice;

            var previousPrices = lastIndex.MiddlePrices;
            return previousPrices.ContainsKey(asset)  // previous prices found in DB in previous IndexState?
                ? previousPrices[asset]               // yes, use them
                : currentMiddlePrice;                 // no, use current
        }
    }
}
