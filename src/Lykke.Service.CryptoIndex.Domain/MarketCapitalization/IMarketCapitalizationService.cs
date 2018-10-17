using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.CryptoIndex.Domain.MarketCapitalization
{
    public interface IMarketCapitalizationService
    {
        Task<IReadOnlyList<AssetMarketCap>> GetAllAsync();
    }
}
