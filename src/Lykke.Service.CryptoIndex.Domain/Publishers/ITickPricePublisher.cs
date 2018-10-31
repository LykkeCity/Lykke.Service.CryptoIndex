using Lykke.Service.CryptoIndex.Domain.Models;

namespace Lykke.Service.CryptoIndex.Domain.Publishers
{
    public interface ITickPricePublisher
    {
        void Publish(IndexTickPrice tickPrice);
    }
}
