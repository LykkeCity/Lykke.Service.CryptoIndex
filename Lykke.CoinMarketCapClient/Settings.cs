using System;

namespace Lykke.CoinMarketCap.Client
{
    public class Settings
    {
        public string ApiKey { get; }

        public string BaseAddress { get; } = "https://pro-api.coinmarketcap.com/v1";

        public TimeSpan TimeOut { get; } = new TimeSpan(0, 0, 0, 10);

        public Settings(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey)) throw new ArgumentOutOfRangeException($"Argument '{nameof(apiKey)}' can't be null or empty.");

            ApiKey = apiKey;
        }

        public Settings(string apiKey, string baseAddress, TimeSpan? timeOut = null) : this(apiKey)
        {
            if (!string.IsNullOrWhiteSpace(baseAddress))
            {
                baseAddress = baseAddress[baseAddress.Length - 1] == '/' ? baseAddress.Substring(0, baseAddress.Length - 1) : baseAddress;
                BaseAddress = baseAddress;
            }

            if (timeOut != null && timeOut < new TimeSpan(0, 0, 0, 1) || timeOut > new TimeSpan(0, 0, 5, 0))
                throw new ArgumentOutOfRangeException($"Argument '{nameof(timeOut)}' must be between 1 second and 5 minutes.");

            BaseAddress = baseAddress;
        }
    }
}
