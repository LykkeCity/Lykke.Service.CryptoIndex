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
        /// The name of the index source.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// The name of the index.
        /// </summary>
        public string AssetPair { get; set; }

        /// <summary>
        /// The price of the index (equals to <see cref="Ask"/>).
        /// </summary>
        public decimal Bid { get; set; }

        /// <summary>
        /// The price of the index (equals to <see cref="Bid"/>).
        /// </summary>
        public decimal Ask { get; set; }

        /// <summary>
        /// The timestamp of the index price. 
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// A collection of weights of assets in the current index.
        /// </summary>
        public IDictionary<string, decimal> Weights { get; set; }
    }
}
