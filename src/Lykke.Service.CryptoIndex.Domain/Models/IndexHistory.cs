using System;
using System.Collections.Generic;
using System.Linq;

namespace Lykke.Service.CryptoIndex.Domain.Models
{
    public class IndexHistory
    {
        /// <summary>
        /// Index value
        /// </summary>
        public decimal Value { get; }

        /// <summary>
        /// Market caps
        /// </summary>
        public IReadOnlyList<AssetMarketCap> MarketCaps { get; }

        /// <summary>
        /// Weights
        /// </summary>
        public IDictionary<string, decimal> Weights { get; }

        /// <summary>
        /// Usd only prices
        /// </summary>
        [Obsolete("Use AssetPrices instead.")]
        public IDictionary<string, IDictionary<string, decimal>> Prices { get; }

        /// <summary>
        /// Row tick prices
        /// </summary>
        public IReadOnlyCollection<TickPrice> TickPrices { get; }

        /// <summary>
        /// Asset prices, including cross
        /// </summary>
        public IReadOnlyCollection<AssetPrice> AssetPrices { get; }

        /// <summary>
        /// Final used asset prices
        /// </summary>
        public IDictionary<string, decimal> MiddlePrices { get; }

        /// <summary>
        /// Assets settings
        /// </summary>
        public IReadOnlyCollection<AssetSettings> AssetsSettings { get; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Time { get; }

        /// <inheritdoc />
        public IndexHistory(
            decimal value,
            IReadOnlyList<AssetMarketCap> marketCaps,
            IDictionary<string, decimal> weights,
            IReadOnlyCollection<TickPrice> tickPrices,
            IReadOnlyCollection<AssetPrice> assetPrices,
            IDictionary<string, decimal> middlePrices,
            DateTime time,
            IReadOnlyCollection<AssetSettings> assetsSettings)
        {
            Value = value == default(decimal) ? throw new ArgumentOutOfRangeException(nameof(value)) : value;
            MarketCaps = marketCaps ?? throw new ArgumentNullException(nameof(marketCaps));
            Weights = weights ?? throw new ArgumentNullException(nameof(weights));
            TickPrices = tickPrices ?? throw new ArgumentNullException(nameof(tickPrices));
            AssetPrices = assetPrices ?? throw new ArgumentNullException(nameof(assetPrices));
            MiddlePrices = middlePrices ?? throw new ArgumentNullException(nameof(middlePrices));
            Time = time == default(DateTime) ? throw new ArgumentOutOfRangeException(nameof(time)) : time.WithoutMilliseconds();
            AssetsSettings = assetsSettings;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Value}, {Time}";
        }
    }
}
