namespace Lykke.Service.CryptoIndex.Domain.Models
{
    /// <summary>
    /// Key Numbers statistics
    /// </summary>
    public class KeyNumbers
    {
        /// <summary>
        /// Last index value
        /// </summary>
        public decimal CurrentValue { get; set; }

        /// <summary>
        /// Return for the last 24 hours
        /// </summary>
        public decimal Return24H { get; set; }

        /// <summary>
        /// Return for the last 5 days
        /// </summary>
        public decimal Return5D { get; set; }

        /// <summary>
        /// Return for the last 30 days
        /// </summary>
        public decimal Return30D { get; set; }

        /// <summary>
        /// Maximum value for the last 24 hours
        /// </summary>
        public decimal Max24H { get; set; }

        /// <summary>
        /// Minimum value for the last 24 hours
        /// </summary>
        public decimal Min24H { get; set; }

        /// <summary>
        /// Volatility for the last 24 hours
        /// </summary>
        public decimal Volatility24H { get; set; }

        /// <summary>
        /// Volatility for the last 30 days
        /// </summary>
        public decimal Volatility30D { get; set; }
    }
}
