using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.CryptoIndex.Domain.Handlers;
using Lykke.Service.CryptoIndex.Domain.Models;

namespace Lykke.Service.CryptoIndex.RabbitMq.Subscribers
{
    internal sealed class TickPricesSubscriber : IStartable, IStopable
    {
        private const string QueuePostfix = ".CryptoIndex";
        private readonly string _connectionString;
        private readonly string _exchangeName;
        private RabbitMqSubscriber<Models.TickPrice> _subscriber;

        private readonly ITickPriceHandler[] _tickPriceHandlers;
        private readonly ILogFactory _logFactory;
        private readonly ILog _log;

        public TickPricesSubscriber(
            string connectionString,
            string exchangeName,
            ITickPriceHandler[] tickPriceHandlers,
            ILogFactory logFactory)
        {
            _connectionString = connectionString;
            _exchangeName = exchangeName;
            
            _tickPriceHandlers = tickPriceHandlers;
            _logFactory = logFactory;
            _log = logFactory.CreateLog(this);
        }

        public void Start()
        {
            var settings = new RabbitMqSubscriptionSettings
            {
                ConnectionString = _connectionString,
                ExchangeName = _exchangeName,
                QueueName = _exchangeName + QueuePostfix,
                IsDurable = false
            };

            _subscriber = new RabbitMqSubscriber<Models.TickPrice>(_logFactory, settings,
                    new ResilientErrorHandlingStrategy(_logFactory, settings, TimeSpan.FromSeconds(10)))
                .SetMessageDeserializer(new JsonMessageDeserializer<Models.TickPrice>())
                .SetMessageReadStrategy(new MessageReadQueueStrategy())
                .Subscribe(ProcessMessageAsync)
                .CreateDefaultBinding()
                .Start();
        }

        public void Stop()
        {
            _subscriber?.Stop();
        }

        public void Dispose()
        {
            _subscriber?.Dispose();
        }

        private async Task ProcessMessageAsync(Models.TickPrice tickPrice)
        {
            var domain = new TickPrice(tickPrice.Source, tickPrice.AssetPair, tickPrice.Bid, tickPrice.Ask, tickPrice.Timestamp);

            await Task.WhenAll(_tickPriceHandlers.Select(o => o.HandleAsync(domain)));
        }
    }
}
