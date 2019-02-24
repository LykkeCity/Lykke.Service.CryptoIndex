using System;
using Lykke.AzureStorage.Tables;

namespace Lykke.Service.CryptoIndex.Domain.Repositories.Models
{
    /// <summary>
    /// History point
    /// </summary>
    public class HistoryPointEntity : AzureTableEntity
    {
        /// <summary>
        /// Index value
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Time { get; set; }

        /// <inheritdoc />
        public HistoryPointEntity()
        {
        }

        /// <inheritdoc />
        public HistoryPointEntity(DateTime time, decimal value)
        {
            Value = value;
            Time = time;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Value}, {Time}";
        }
    }
}
