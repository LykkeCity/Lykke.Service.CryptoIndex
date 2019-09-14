using Autofac;
using AzureStorage.Blob;
using AzureStorage.Tables;
using Common;
using JetBrains.Annotations;
using Lykke.CoinMarketCap.Client;
using Lykke.Common.Log;
using Lykke.Service.CryptoIndex.Domain.Handlers;
using Lykke.Service.CryptoIndex.Domain.Models;
using Lykke.Service.CryptoIndex.Domain.Repositories;
using Lykke.Service.CryptoIndex.Domain.Repositories.Models;
using Lykke.Service.CryptoIndex.Domain.Repositories.Repositories;
using Lykke.Service.CryptoIndex.Domain.Services;
using Lykke.Service.CryptoIndex.Domain.Services.Publishers;
using Lykke.Service.CryptoIndex.RabbitMq.Publishers;
using Lykke.Service.CryptoIndex.RabbitMq.Subscribers;
using Lykke.Service.CryptoIndex.Settings;
using Lykke.SettingsReader;

namespace Lykke.Service.CryptoIndex.Modules
{
    [UsedImplicitly]
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _appSettings;
        private readonly CryptoIndexSettings _settings;
        private readonly IReloadingManager<string> _connectionString;

        public ServiceModule(IReloadingManager<AppSettings> appSettings)
        {
            _appSettings = appSettings;
            _settings = _appSettings.CurrentValue.CryptoIndexService;
            _connectionString = _appSettings.Nested(x => x.CryptoIndexService.Db.DataConnectionString);
        }

        protected override void Load(ContainerBuilder builder)
        {
            // RabbitMq

            foreach (var exchange in _settings.RabbitMq.SubscribingExchanges)
            {
                builder.RegisterType<TickPricesSubscriber>()
                    .AsSelf()
                    .As<IStartable>()
                    .As<IStopable>()
                    .WithParameter("connectionString", _settings.RabbitMq.ConnectionString)
                    .WithParameter("exchangeName", exchange)
                    .WithParameter("suffixName", _settings.IndexName)
                    .SingleInstance();
            }

            builder.RegisterType<TickPricePublisher>()
                .As<ITickPricePublisher>()
                .As<IStartable>()
                .As<IStopable>()
                .WithParameter(TypedParameter.From(_settings.RabbitMq))
                .SingleInstance();

            // Repositories

            // Blob

            builder.RegisterInstance(AzureBlobStorage.Create(_connectionString));
            builder.RegisterType<IndexHistoryBlobRepository>();

            // Tables

            builder.Register(c => new SettingsRepository(
                    AzureTableStorage<SettingsEntity>.Create(_connectionString,
                        nameof(Settings), c.Resolve<ILogFactory>())))
                .As<ISettingsRepository>()
                .SingleInstance();

            builder.Register(c => new IndexHistoryRepository(
                    AzureTableStorage<IndexHistoryEntity>.Create(_connectionString,
                        nameof(IndexHistory), c.Resolve<ILogFactory>()), c.Resolve<IndexHistoryBlobRepository>()))
                .As<IIndexHistoryRepository>()
                .SingleInstance();

            builder.Register(c => new IndexStateRepository(
                    AzureTableStorage<IndexStateEntity>.Create(_connectionString,
                        nameof(IndexState), c.Resolve<ILogFactory>())))
                .As<IIndexStateRepository>()
                .SingleInstance();

            builder.Register(c => new FirstStateAfterResetTimeRepository(
                    AzureTableStorage<FirstStateAfterResetTimeEntity>.Create(_connectionString,
                        "FirstStateAfterResetTime", c.Resolve<ILogFactory>())))
                .As<IFirstStateAfterResetTimeRepository>()
                .SingleInstance();

            builder.Register(c => new WarningRepository(
                    AzureTableStorage<WarningEntity>.Create(_connectionString,
                        nameof(Warning), c.Resolve<ILogFactory>())))
                .As<IWarningRepository>()
                .SingleInstance();

            builder.Register(c => new ChartHistory5DRepository(
                    AzureTableStorage<HistoryPointEntity>.Create(_connectionString,
                        "ChartHistory5D", c.Resolve<ILogFactory>())))
                .As<IChartHistory5DRepository>()
                .SingleInstance();

            builder.Register(c => new ChartHistory30DRepository(
                    AzureTableStorage<HistoryPointEntity>.Create(_connectionString,
                        "ChartHistory30D", c.Resolve<ILogFactory>())))
                .As<IChartHistory30DRepository>()
                .SingleInstance();

            // Services

            builder.RegisterType<TickPricesService>()
                .As<ITickPricesService>()
                .As<ITickPriceHandler>()
                .SingleInstance();

            builder.RegisterType<CoinMarketCapClient>()
                .As<ICoinMarketCapClient>()
                .WithParameter(TypedParameter.From(new CoinMarketCap.Client.Settings(_settings.CoinMarketCapApiKey)))
                .SingleInstance();

            builder.RegisterType<CoinMarketCapService>()
                .As<ICoinMarketCapService>()
                .SingleInstance();

            builder.RegisterType<SettingsService>()
                .As<ISettingsService>()
                .WithParameter("indexTickPriceAssetPair", _settings.IndexName)
                .SingleInstance();

            builder.RegisterType<IndexCalculator>()
                .As<IIndexCalculator>()
                .As<IStartable>()
                .As<IStopable>()
                .WithParameter("indexName", _settings.IndexName)
                .WithParameter("shortIndexName", _settings.ShortIndexName)
                .WithParameter("isShortIndexEnabled", _settings.IsShortIndexEnabled)
                .WithParameter("indexCalculationInterval", _settings.IndexCalculationInterval)
                .SingleInstance();

            builder.RegisterType<StatisticsService>()
                .As<IStatisticsService>()
                .As<IIndexHandler>()
                .SingleInstance();
        }
    }
}
