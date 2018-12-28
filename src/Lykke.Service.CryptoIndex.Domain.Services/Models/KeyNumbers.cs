namespace Lykke.Service.CryptoIndex.Domain.Services.Models
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
        public decimal Return24h { get; set; }

        /// <summary>
        /// Return for the last 5 days
        /// </summary>
        public decimal Return5d { get; set; }

        /// <summary>
        /// Return for the last 30 days
        /// </summary>
        public decimal Return30d { get; set; }

        /// <summary>
        /// Maximum value for the last 24 hours
        /// </summary>
        public decimal Max24h { get; set; }

        /// <summary>
        /// Minimum value for the last 24 hours
        /// </summary>
        public decimal Min24h { get; set; }

        /// <summary>
        /// Volatility for the last 24 hours
        /// </summary>
        public decimal Volatility24h { get; set; }

        /// <summary>
        /// Volatility for the last 30 days
        /// </summary>
        public decimal Volatility30d { get; set; }
    }
}
