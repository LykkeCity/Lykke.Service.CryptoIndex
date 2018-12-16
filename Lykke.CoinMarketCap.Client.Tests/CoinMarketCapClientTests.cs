using System.Linq;
using Lykke.Logs;
using Xunit;

namespace Lykke.CoinMarketCap.Client.Tests
{
    public class CryptoCurrencyClientTests
    {
        private const string ApiKey = "";

        private readonly ICryptoCurrencyClient _client = new CoinMarketCapClient(new Settings(ApiKey), LogFactory.Create()).CryptoCurrencyClient;

        //[Fact]
        public void ListingsLatestTest()
        {
            var result = _client.GetListingsLatestAsync().GetAwaiter().GetResult();

            Assert.NotEmpty(result.Data);
            Assert.True(result.Data.Count() > 10);
            Assert.NotEmpty(result.Data.First().Quotes);
            Assert.NotEqual(default(decimal), result.Data.First().CirculatingSupply);
        }
    }
}
