using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Lykke.Service.CryptoIndex.Client.Models
{
    /// <summary>
    /// Represents a snapshot of one index calculation
    /// </summary>
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class IndexHistory
    {
        /// <summary>
        /// Index value
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// Assets market capitalization
        /// </summary>
        public IReadOnlyList<AssetMarketCap> MarketCaps { get; set; }

        /// <summary>
        /// Weights
        /// </summary>
        public IDictionary<string, decimal> Weights { get; set; }

        /// <summary>
        /// Usd only prices
        /// </summary>
        [Obsolete("Use AssetPrices instead.")] // check dependencies in IndicesFacade and lykke.com
        public IDictionary<string, IDictionary<string, decimal>> Prices { get; set; }

        /// <summary>
        /// Row tick prices
        /// </summary>
        public IReadOnlyCollection<TickPrice> TickPrices { get; set; }

        /// <summary>
        /// Asset prices, including cross
        /// </summary>
        public IReadOnlyCollection<AssetPrice> AssetPrices { get; set; }

        /// <summary>
        /// Final used asset prices
        /// </summary>
        public IDictionary<string, decimal> MiddlePrices { get; set; }

        /// <summary>
        /// Assets settings
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
