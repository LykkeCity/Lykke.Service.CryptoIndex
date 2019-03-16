using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Service.CryptoIndex.Domain.Models;

namespace Lykke.Service.CryptoIndex.Domain.Services
{
    public static class Utils
    {
        public static decimal GetMiddlePrice(string asset, IReadOnlyCollection<AssetPrice> assetPrices)
        {
            if (assetPrices == null || assetPrices.Count == 0)
                throw new ArgumentOutOfRangeException($"Asset '{asset}' doesn't have any prices.");

            var prices = assetPrices.Select(x => x.Price).OrderBy(x => x).ToList();

            if (prices.Count > 2)
            {
                prices.RemoveAt(0);
                prices.RemoveAt(prices.Count - 1);
            }

            var middlePrice = prices.Sum() / prices.Count;

            middlePrice = Math.Round(middlePrice, 8);

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
