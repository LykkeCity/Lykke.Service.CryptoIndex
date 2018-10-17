using Autofac;
using Common;

namespace Lykke.Service.CryptoIndex.Domain.TickPrice
{
    public interface ITickPricePublisher : IStartable, IStopable
    {
        void Publish(TickPrice tickPrice);
    }
}
