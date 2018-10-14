using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.CryptoIndex.Domain.MarketCapitalization
{
    public interface IMarketCapitalizationService : IDisposable
    {
        Task<IReadOnlyList<AssetMarketCap>> GetAllAsync();
    }
}
