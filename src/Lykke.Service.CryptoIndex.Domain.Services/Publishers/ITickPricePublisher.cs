using Lykke.Service.CryptoIndex.Contract;

namespace Lykke.Service.CryptoIndex.Domain.Services.Publishers
{
    public interface ITickPricePublisher
    {
        void Publish(IndexTickPrice tickPrice);
    }
}
