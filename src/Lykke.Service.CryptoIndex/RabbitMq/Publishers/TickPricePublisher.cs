using Autofac;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.CryptoIndex.Domain.TickPrice;
using Lykke.Service.CryptoIndex.Settings;
using TickPrice = Lykke.Service.CryptoIndex.RabbitMq.Models.TickPrice;
using DomainTickPrice = Lykke.Service.CryptoIndex.Domain.TickPrice.TickPrice;

namespace Lykke.Service.CryptoIndex.RabbitMq.Publishers
{
    [UsedImplicitly]
    public class TickPricePublisher : ITickPricePublisher, IStartable, IStopable
    {
        private readonly ILogFactory _logFactory;
        private readonly RabbitMqSettings _settings;
        private RabbitMqPublisher<TickPrice> _publisher;
        private readonly ILog _log;

        public TickPricePublisher(RabbitMqSettings settings, ILogFactory logFactory)
        {
            _logFactory = logFactory;
            _settings = settings;
            _log = logFactory.CreateLog(this);
        }

        public void Dispose()
        {
            _publisher?.Dispose();
        }

        public void Publish(DomainTickPrice tickPrice)
        {
            _publisher.ProduceAsync(Convert(tickPrice));
            _log.Info($"Published tick price: {tickPrice.ToJson()}.");
        }

        private static TickPrice Convert(DomainTickPrice tickPrice)
        {
            return new TickPrice
            {
                Source = tickPrice.Source,
                Timestamp = tickPrice.Timestamp,
                AssetPair = tickPrice.AssetPair,
                Ask = tickPrice.Ask,
                Bid = tickPrice.Bid
            };
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings
                .ForPublisher(_settings.ConnectionString, _settings.PublishingExchange);

            _publisher = new RabbitMqPublisher<TickPrice>(_logFactory, settings)
                .SetSerializer(new JsonMessageSerializer<TickPrice>())
                .DisableInMemoryQueuePersistence()
                .Start();
        }

        public void Stop()
        {
            _publisher?.Stop();
        }
    }
}
