using System;
using System.Collections.Generic;
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
        /// All prices
        /// </summary>
        public IDictionary<string, IDictionary<string, decimal>> Prices { get; set; }

        /// <summary>
        /// Middle prices
        /// </summary>
        public IDictionary<string, decimal> MiddlePrices { get; set; }

        /// <summary>
        /// Frozen assets
        /// </summary>
        [Obsolete]
        public IReadOnlyCollection<string> FrozenAssets { get; set; }

        /// <summary>
        /// List of frozen assets
        /// </summary>
        public IReadOnlyList<AssetSettings> AssetsSettings { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Time { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Value}, {Time}";
        }
    }
}
