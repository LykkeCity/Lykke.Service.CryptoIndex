using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.CryptoIndex.Domain.TickPrice
{
    public interface ITickPricesService
    {
        Task<IDictionary<string, IDictionary<string, decimal>>> GetPricesAsync();
    }
}
