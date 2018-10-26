using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Common.Log;
using Lykke.CoinMarketCap.Client.Models.CryptoCurrency;
using Lykke.Common.Log;
using Newtonsoft.Json;

namespace Lykke.CoinMarketCap.Client
{
    public class CryptoCurrencyClient : ICryptoCurrencyClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILog _log;

        public CryptoCurrencyClient(HttpClient httpClient, ILogFactory logFactory)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            _log = logFactory.CreateLog(this);
        }

        public async Task<BaseResponse<ListingsLatestResponse[]>> GetListingsLatestAsync(int? start = null, int? limit = null, string[] convert = null, string sortField = null,
            string sortDir = null, string cryptoCurrencyType = null, CancellationToken ct = default(CancellationToken))
        {
            var query = HttpUtility.ParseQueryString(string.Empty);

            if (start.HasValue)
                query["start"] = start.Value.ToString();

            if (limit.HasValue)
                query["limit"] = limit.Value.ToString();

            if (convert != null && convert.Any())
                query["convert"] = $"[{string.Join(",", convert)}]";

            if (!string.IsNullOrWhiteSpace(sortDir))
                query["sortDir"] = sortDir;

            if (!string.IsNullOrWhiteSpace(cryptoCurrencyType))
                query["cryptoCurrencyType"] = cryptoCurrencyType;

            var queryString = query.ToString();
            queryString = string.IsNullOrWhiteSpace(queryString) ? string.Empty : $"?{queryString}";

            try
            {
                using (var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/cryptocurrency/listings/latest{queryString}", ct))
                {
                    var responseStr = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<BaseResponse<ListingsLatestResponse[]>>(responseStr);

                    return result;
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
                throw;
            }
        }
    }
}
