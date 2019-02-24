using System;

namespace Lykke.Service.CryptoIndex.Domain.Models
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
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
            if (string.IsNullOrWhiteSpace(asset)) throw new ArgumentException(nameof(asset));

            Value = value;
            Asset = asset;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Value} {Asset}";
        }
    }
}
