using System;

namespace Lykke.Service.CryptoIndex.Domain.Models
{
    public class MarketCap
    {
        public decimal Value { get; }

        public string Asset { get; }

        public MarketCap(decimal value, string asset)
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
            if (string.IsNullOrWhiteSpace(asset)) throw new ArgumentOutOfRangeException(nameof(asset));

            Value = value;
            Asset = asset;
        }

        public override string ToString()
        {
            return $"{Value} {Asset}";
        }
    }
}
