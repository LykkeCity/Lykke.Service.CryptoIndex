using System;

namespace Lykke.Service.CryptoIndex.Domain.Models
{
    /// <summary>
    /// Warning message
    /// </summary>
    public class Warning
    {
        /// <summary>
        /// Message
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Time { get; }

        /// <inheritdoc />
        public Warning(string message, DateTime time)
        {
            Message = message;
            Time = time;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Time} - {Message}";
        }
    }
}
