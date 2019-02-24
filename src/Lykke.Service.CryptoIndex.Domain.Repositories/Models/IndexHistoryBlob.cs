using System;
using System.Collections.Generic;
using System.Linq;

namespace Lykke.Service.CryptoIndex.Domain.Repositories.Models
{
    /// <summary>
    /// Blob for storing original tick prices and cross prices while index calculation.
    /// </summary>
    public class IndexHistoryBlob
    {
        /// <summary>
        /// Assets USD prices, old format, saved for compatibility reason
        /// </summary>
        [Obsolete("Used for existed usd prices only, use AssetPrices instead.")]
        public IDictionary<string, IDictionary<string, decimal>> Prices { get; set; }
            = new Dictionary<string, IDictionary<string, decimal>>();

        /// <summary>
        /// Raw tick prices
        /// </summary>
        public IReadOnlyCollection<TickPriceEntity> TickPrices { get; set; } = new List<TickPriceEntity>();

        /// <summary>
        /// Usd and cross prices, new format
        /// </summary>
        public IReadOnlyCollection<AssetPriceEntity> AssetPrices { get; set; } = new List<AssetPriceEntity>();

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Time { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{AssetPrices?.Count}, {TickPrices?.Count}, old={Prices?.Count}, {Time}";
        }

        /// <summary>
        /// Returns AssetPrice's from <see cref="Prices"/> or <see cref="AssetPrices"/>
        /// </summary>
        public IReadOnlyCollection<AssetPriceEntity> GetAssetPrices()
        {
            if (AssetPrices.Any())
                return AssetPrices;

            var result = new List<AssetPriceEntity>();

            foreach (var assetSourcePrice in Prices)
            {
                foreach (var sourcePrice in assetSourcePrice.Value)
                {
                    var newAssetPrice = new AssetPriceEntity
                    {
                        Asset = assetSourcePrice.Key,
                        CrossAsset = "USD", // old format contains USD prices only
                        Source = sourcePrice.Key,
                        Price = sourcePrice.Value
                    };

                    result.Add(newAssetPrice);
                }
            }

            return result;
        }
    }
}
