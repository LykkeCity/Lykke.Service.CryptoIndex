using System;

namespace Lykke.Service.CryptoIndex.Client.Models
{
    /// <summary>
    /// Market capitalization
    /// </summary>
    public class MarketCap
    {
        /// <summary>
        /// Value
        /// </summary>
        public decimal Value { get; }

        /// <summary>
        /// Unit
        /// </summary>
        public string Asset { get; }

        /// <inheritdoc />
        public MarketCap(decimal value, string asset)
        {
            if (value == default(decimal)) throw new ArgumentOutOfRangeException(nameof(value));
            if (string.IsNullOrWhiteSpace(asset)) throw new ArgumentOutOfRangeException(nameof(asset));

            Value = value;
            Asset = asset;
        }
    }
}
