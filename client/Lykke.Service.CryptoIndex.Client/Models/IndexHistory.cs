using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Lykke.Service.CryptoIndex.Client.Models
{
    /// <summary>
    /// Index history element
    /// </summary>
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class IndexHistory
    {
        /// <summary>
        /// Index value
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// Assets Market Cap
        /// </summary>
        public IReadOnlyList<AssetMarketCap> MarketCaps { get; set; }

        /// <summary>
        /// Weights
        /// </summary>
        public IDictionary<string, decimal> Weights { get; set; }

        /// <summary>
        /// Usd only prices
        /// </summary>
        [Obsolete("Use GetAssetPrices instead.")]
        public IDictionary<string, IDictionary<string, decimal>> Prices { get; set; }

        /// <summary>
        /// Row tick prices
        /// </summary>
        public IReadOnlyCollection<TickPrice> TickPrices { get; set; }

        /// <summary>
        /// Middle prices, including cross
        /// </summary>
        [Obsolete("Use GetAssetPrices instead.")]
        public IReadOnlyCollection<AssetPrice> AssetPrices { get; set; }

        /// <summary>
        /// Middle prices
        /// </summary>
        public IDictionary<string, decimal> MiddlePrices { get; set; }

        /// <summary>
        /// List of frozen assets
        /// </summary>
        public IReadOnlyList<AssetSettings> AssetsSettings { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Returns asset prices including crosses
        /// </summary>
        /// <returns></returns>
        public IReadOnlyCollection<AssetPrice> GetAssetPrices()
        {
            if (AssetPrices != null && AssetPrices.Any())
                return AssetPrices;

            var result = new List<AssetPrice>();

            if (Prices == null)
                return result;

            foreach (var assetSourcePrice in Prices)
            {
                foreach (var sourcePrice in assetSourcePrice.Value)
                {
                    var newAssetPrice = new AssetPrice
                    {
                        Asset = assetSourcePrice.Key,
                        CrossAsset = "USD",
                        Source = sourcePrice.Key,
                        Price = sourcePrice.Value
                    };

                    result.Add(newAssetPrice);
                }
            }

            return result;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Value}, {Time}";
        }
    }
}
