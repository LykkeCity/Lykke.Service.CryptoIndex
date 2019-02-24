using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Lykke.Service.CryptoIndex.Contract
{
    /// <summary>
    /// Represent the index tick price.
    /// </summary>
    [PublicAPI]
    public class IndexTickPrice
    {
        /// <summary>
        /// Name of the index source.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Name of the index.
        /// </summary>
        public string AssetPair { get; set; }

        /// <summary>
        /// Price of the index (equals to <see cref="Ask"/>).
        /// </summary>
        public decimal? Bid { get; set; }

        /// <summary>
        /// Price of the index (equals to <see cref="Bid"/>).
        /// </summary>
        public decimal? Ask { get; set; }

        /// <summary>
        /// Timestamp of the index price. 
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Collection of assets information.
        /// </summary>
        public IReadOnlyCollection<AssetInfo> AssetsInfo { get; set; }

        /// <inheritdoc />
        public IndexTickPrice()
        {
        }

        /// <inheritdoc />
        public IndexTickPrice(string source, string assetPair, decimal? bid, decimal? ask, DateTime timestamp, IReadOnlyCollection<AssetInfo> assetsInfo)
        {
            Source = source;
            AssetPair = assetPair;
            Bid = bid;
            Ask = ask;
            Timestamp = timestamp;
            AssetsInfo = assetsInfo;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Source}, {AssetPair}, ask={Ask}, bid={Bid}, {Timestamp}, {AssetsInfo?.Count}";
        }
    }
}
