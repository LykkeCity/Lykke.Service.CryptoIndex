using System;

namespace Lykke.CoinMarketCap.Client
{
    public interface ICoinMarketCapClient : IDisposable
    {
        ICryptoCurrencyClient CryptoCurrencyClient { get; }
    }
}
