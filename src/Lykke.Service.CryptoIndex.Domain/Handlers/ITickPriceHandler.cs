using System.Threading.Tasks;
using Lykke.Service.CryptoIndex.Domain.Models;

namespace Lykke.Service.CryptoIndex.Domain.Handlers
{
    public interface ITickPriceHandler
    {
        Task HandleAsync(TickPrice tickPrice);
    }
}
