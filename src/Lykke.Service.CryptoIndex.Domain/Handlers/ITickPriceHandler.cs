using System.Threading.Tasks;

namespace Lykke.Service.CryptoIndex.Domain.Handlers
{
    public interface ITickPriceHandler
    {
        Task HandleAsync(Models.TickPrice tickPrice);
    }
}
