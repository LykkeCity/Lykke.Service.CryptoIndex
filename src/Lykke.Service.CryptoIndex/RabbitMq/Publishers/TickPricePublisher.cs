using System.Linq;
using Autofac;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.CryptoIndex.Domain.Services.Publishers;
using Lykke.Service.CryptoIndex.Settings;

namespace Lykke.Service.CryptoIndex.RabbitMq.Publishers
{
    [UsedImplicitly]
    public class TickPricePublisher : ITickPricePublisher, IStartable, IStopable
    {
        private readonly ILogFactory _logFactory;
        private readonly RabbitMqSettings _settings;
        private RabbitMqPublisher<Contract.IndexTickPrice> _publisher;
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

        public void Publish(Contract.IndexTickPrice tickPrice)
        {
            _publisher.ProduceAsync(new Contract.IndexTickPrice
            {
                Source = tickPrice.Source,
                AssetPair = tickPrice.AssetPair,
                Ask = tickPrice.Ask ?? 0,
                Bid = tickPrice.Bid ?? 0,
                Timestamp = tickPrice.Timestamp,
                AssetsInfo = tickPrice.AssetsInfo.Select(x => new Contract.AssetInfo(x.AssetId, x.Weight, x.Price, x.IsDisabled)).ToList()
            });

            _log.Info($"Published tick price: {tickPrice.ToJson()}.");
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings
                .ForPublisher(_settings.ConnectionString, _settings.PublishingExchange);

            _publisher = new RabbitMqPublisher<Contract.IndexTickPrice>(_logFactory, settings)
                .SetSerializer(new JsonMessageSerializer<Contract.IndexTickPrice>())
                .DisableInMemoryQueuePersistence()
                .Start();
        }

        public void Stop()
        {
            _publisher?.Stop();
        }
    }
}
