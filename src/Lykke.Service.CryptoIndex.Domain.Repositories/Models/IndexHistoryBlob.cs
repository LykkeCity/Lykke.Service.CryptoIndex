using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Lykke.Service.CryptoIndex.Domain.Models;

namespace Lykke.Service.CryptoIndex.Domain.Repositories.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class IndexHistoryBlob
    {
        private const string Usd = "USD";

        /// <summary>
        /// Assets USD prices
        /// </summary>
        [Obsolete("Used for existed usd prices only, use AssetPrices instead.")]
        public IDictionary<string, IDictionary<string, decimal>> Prices { get; set; }
            = new Dictionary<string, IDictionary<string, decimal>>();

        /// <summary>
        /// Raw tick prices
        /// </summary>
        public IReadOnlyCollection<TickPriceEntity> TickPrices { get; set; } = new List<TickPriceEntity>();

        /// <summary>
        /// Usd and cross prices
        /// </summary>
        public IReadOnlyCollection<AssetPriceEntity> AssetPrices { get; set; } = new List<AssetPriceEntity>();

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Time { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Time}";
        }

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
                        CrossAsset = "USD",
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
