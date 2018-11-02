using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.CryptoIndex.Domain.Services
{
    public interface IIndexCalculator
    {
        Task<IReadOnlyDictionary<string, decimal>> GetAllAssetsMarketCapsAsync();

        Task Reset();
    }
}
