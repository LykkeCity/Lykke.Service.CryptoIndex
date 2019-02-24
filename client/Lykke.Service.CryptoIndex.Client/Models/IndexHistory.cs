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
        [Obsolete("Use AssetPrices instead.")]
        public IDictionary<string, IDictionary<string, decimal>> Prices { get; set; }

        /// <summary>
        /// Row tick prices
        /// </summary>
        public IReadOnlyCollection<TickPrice> TickPrices { get; set; }

        /// <summary>
        /// Middle prices, including cross
        /// </summary>
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

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Value}, {Time}";
        }
    }
}
