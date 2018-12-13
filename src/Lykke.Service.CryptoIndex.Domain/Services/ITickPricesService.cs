using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.CryptoIndex.Domain.Services
{
    public interface ITickPricesService
    {
        Task<IDictionary<string, IDictionary<string, decimal>>> GetPricesAsync(ICollection<string> sources = null);
    }
}
