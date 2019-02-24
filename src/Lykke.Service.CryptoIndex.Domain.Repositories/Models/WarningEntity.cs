using System;
using Lykke.AzureStorage.Tables;

namespace Lykke.Service.CryptoIndex.Domain.Repositories.Models
{
    /// <summary>
    /// Warning message
    /// </summary>
    public class WarningEntity : AzureTableEntity
    {
        /// <summary>
        /// Message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Time { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Time} - {Message}";
        }
    }
}
