using Newtonsoft.Json;

namespace Lykke.CoinMarketCap.Client.Models.CryptoCurrency
{
    public class BaseResponse<T>
    {
        [JsonProperty("data")]
        public T Data { get; set; }

        [JsonProperty("status")]
        public Status Status { get; set; }
    }
}
