namespace Lykke.Service.CryptoIndex.Domain.Repositories.Models
{
    /// <summary>
    /// Market capitalization
    /// </summary>
    public class MarketCapEntity
    {
        /// <summary>
        /// Value
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// Unit
        /// </summary>
        public string Asset { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Value} {Asset}";
        }
    }
}
