using System;

namespace Lykke.Service.CryptoIndex.Client.Models.LCI10
{
    /// <summary>
    /// Market cap info
    /// </summary>
    public class MarketCap
    {
        /// <summary>
        /// Value
        /// </summary>
        public decimal Value { get; }

        /// <summary>
        /// Asset
        /// </summary>
        public string Asset { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public MarketCap(decimal value, string asset)
        {
            if (value == default(decimal)) throw new ArgumentOutOfRangeException(nameof(value));
            if (string.IsNullOrWhiteSpace(asset)) throw new ArgumentOutOfRangeException(nameof(asset));

            Value = value;
            Asset = asset;
        }
    }
}
