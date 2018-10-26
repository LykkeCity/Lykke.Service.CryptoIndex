using System;
using System.Net;
using System.Net.Http;
using Lykke.Common.Log;

namespace Lykke.CoinMarketCap.Client
{
    public class CoinMarketCapClient : ICoinMarketCapClient
    {
        private readonly HttpClient _httpClient;

        public ICryptoCurrencyClient CryptoCurrencyClient { get; }

        public CoinMarketCapClient(Settings settings, ILogFactory logFactory)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(settings.BaseAddress)
            };
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "deflate, gzip");
            _httpClient.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", settings.ApiKey);

            _httpClient.Timeout = settings.TimeOut;

            CryptoCurrencyClient = new CryptoCurrencyClient(_httpClient, logFactory);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
