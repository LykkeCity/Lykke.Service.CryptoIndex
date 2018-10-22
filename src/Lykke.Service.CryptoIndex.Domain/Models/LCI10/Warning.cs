using System;

namespace Lykke.Service.CryptoIndex.Domain.Models.LCI10
{
    public class Warning
    {
        public string Message { get; }

        public DateTime Time { get; }

        public Warning(string message, DateTime time)
        {
            Message = message;
            Time = time;
        }

        public override string ToString()
        {
            return $"{Time} - {Message}";
        }
    }
}
