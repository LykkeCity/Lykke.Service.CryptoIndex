using System.Threading;
using System.Threading.Tasks;
using Lykke.CoinMarketCap.Client.Models.CryptoCurrency;

namespace Lykke.CoinMarketCap.Client
{
    /// <summary>
    /// https://pro.coinmarketcap.com/api/v1#tag/cryptocurrency
    /// </summary>
    public interface ICryptoCurrencyClient
    {
        /// <summary>
        /// https://pro.coinmarketcap.com/api/v1#operation/getV1CryptocurrencyListingsLatest
        /// </summary>
        Task<BaseResponse<ListingsLatestResponse[]>> GetListingsLatestAsync(int? start = null, int? limit = null, string[] convert = null,
            string sortField = null, string sortDir = null, string cryptoCurrencyType = null, CancellationToken ct = default(CancellationToken));
    }
}
