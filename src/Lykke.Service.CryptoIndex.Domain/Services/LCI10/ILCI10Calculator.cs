using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.CryptoIndex.Domain.Services.LCI10
{
    public interface ILCI10Calculator
    {
        Task<IReadOnlyDictionary<string, decimal>> GetAssetMarketCapAsync();
    }
}
