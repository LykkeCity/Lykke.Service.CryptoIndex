using System;
using Lykke.AzureStorage.Tables;

namespace Lykke.Service.CryptoIndex.Domain.Repositories.Models
{
    /// <summary>
    /// Represents a moment when first index was calculated after reset
    /// </summary>
    public class FirstStateAfterResetTimeEntity : AzureTableEntity
    {
        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Time { get; set; }

        public override string ToString()
        {
            return $"{Time}";
        }
    }
}
