namespace Lykke.Service.CryptoIndex.Domain.Publishers
{
    public interface ITickPricePublisher
    {
        void Publish(Models.TickPrice tickPrice);
    }
}
