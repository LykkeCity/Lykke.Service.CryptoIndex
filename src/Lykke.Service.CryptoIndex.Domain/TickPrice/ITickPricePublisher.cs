namespace Lykke.Service.CryptoIndex.Domain.TickPrice
{
    public interface ITickPricePublisher
    {
        void Publish(TickPrice tickPrice);
    }
}
