using System;
using JetBrains.Annotations;

namespace Lykke.Service.CryptoIndex.Client.Models
{
    /// <summary>
    /// Warning element
    /// </summary>
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class Warning
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
            return $"{Time}, {Message}";
        }
    }
}
