using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.CoinMarketCap.Client;
using Lykke.Common.Log;
using Lykke.Service.CryptoIndex.Domain.MarketCapitalization;

namespace Lykke.Service.CryptoIndex.DomainServices.MarketCapitalization
{
    public class MarketCapitalizationService : IMarketCapitalizationService
    {
        private readonly ICoinMarketCapClient _coinMarketCapClient;
        private readonly ILog _log;

        public MarketCapitalizationService(ICoinMarketCapClient coinMarketCapClient, ILogFactory logFactory)
        {
            _coinMarketCapClient = coinMarketCapClient;
            _log = logFactory.CreateLog(this);
        }

        public async Task<IReadOnlyList<AssetMarketCap>> GetAllAsync()
        {
            var result = await _coinMarketCapClient.CryptoCurrencyClient.GetListingsLatestAsync();

            if (result.Status.ErrorCode != 0 || result.Status.ErrorMessage != null)
                _log.Warning($"Get an error while receiving to CoinMarketCap: {result.Status.ErrorCode} - {result.Status.ErrorMessage}.");

            return result.Data.Select(x => new AssetMarketCap(MapSymbol(x.Symbol), new MarketCap(x.Quotes.First().Value.MarketCap, "USD"))).ToList();
        }

        public void Dispose()
        {
            _coinMarketCapClient?.Dispose();
        }

        private string MapSymbol(string symbol)
        {
            // TODO: Replace with a mapping configuration
            return symbol == "MIOTA" ? "IOTA" : symbol;
        }
    }
}
