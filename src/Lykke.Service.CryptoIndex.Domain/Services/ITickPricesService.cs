using System.Collections.Generic;
using Lykke.Service.CryptoIndex.Domain.Models;

namespace Lykke.Service.CryptoIndex.Domain.Services
{
    public interface ITickPricesService
    {
        IDictionary<string, IReadOnlyCollection<TickPrice>> GetTickPrices(IReadOnlyCollection<string> sources = null);

        IDictionary<string, IReadOnlyCollection<AssetPrice>> GetAssetPrices(IReadOnlyCollection<string> sources = null);
    }
}
