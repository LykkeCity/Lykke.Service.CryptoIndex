using System.Threading.Tasks;

namespace Lykke.Service.CryptoIndex.Domain.TickPrice
{
    public interface ITickPriceHandler
    {
        Task HandleAsync(TickPrice tickPrice);
    }
}
