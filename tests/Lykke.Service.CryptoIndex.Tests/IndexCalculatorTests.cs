using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lykke.Logs;
using Lykke.Service.CryptoIndex.Domain.Handlers;
using Lykke.Service.CryptoIndex.Domain.Models;
using Lykke.Service.CryptoIndex.Domain.Repositories;
using Lykke.Service.CryptoIndex.Domain.Services;
using Lykke.Service.CryptoIndex.Domain.Services.Publishers;
using Moq;
using Xunit;

namespace Lykke.Service.CryptoIndex.Tests
{
    public class IndexCalculatorTests
    {
        private const string Usd = "USD";

        // settings

        // for publishing
        private const string LyCi = "LyCI";
        private const string ShortLyCi = "ShortLyCI";

        // calculation interval
        private readonly TimeSpan _timerInterval = TimeSpan.FromSeconds(1);

        private const string SuppliesForEachStep =
            "BTC		XRP		    ETH		    LTC     " + "\r\n" +
            "17434512	40762365544	103925034	59665588" + "\r\n" +
            "17434512	40762365544	103925034	59665588" + "\r\n" +
            "17434512	40762365544	103925034	59665588" ;

        private const string IndicesValuesAndPricesForEachStep =
            "Index		BTC		XRP		ETH		LTC" + "\r\n" +
            "1000		4000	0.42	100		33 " + "\r\n" +
            "1005.52	4001	0.43	101		34 " + "\r\n" +
            "1000.16	4000	0.42	100		33 ";

        private int _step = 0;

        // arrange

        private ISettingsService _settingsService;
        private IIndexStateRepository _indexStateRepository;
        private IIndexHistoryRepository _indexHistoryRepository;
        private IFirstStateAfterResetTimeRepository _firstStateAfterResetTimeRepository;
        private ITickPricesService _tickPricesService;
        private ICoinMarketCapService _coinMarketCapService;
        private ITickPricePublisher _tickPricePublisher;
        private IWarningRepository _warningRepository;
        private IIndexHandler _indexHandler;
        private IndexCalculator _indexCalculator;

        private void InitializeSettingsService()
        {
            const string lykke = "lykke";
            const string bitstamp = "bitstamp";

            IReadOnlyList<string> sources = new List<string> { lykke, bitstamp };

            IReadOnlyList<string> assets = GetSupply(0).Keys.ToList();

            const int topCount = 3;

            const bool enabled = true;

            IReadOnlyList<AssetSettings> assetsSettings = new List<AssetSettings>();

            var settingsServiceMock = new Mock<ISettingsService>();
            settingsServiceMock.Setup(o => o.GetAsync())
                .Returns(() => Task.FromResult(new Settings
                {
                    Sources = sources,
                    Assets = assets,
                    TopCount = topCount,
                    Enabled = enabled,
                    RebuildTime = TimeSpan.Zero,
                    AssetsSettings = assetsSettings,
                    AutoFreezeChangePercents = 0
                }));

            _settingsService = settingsServiceMock.Object;
        }

        private IndexState _lastIndexState;
        private void InitializeIndexStateRepository()
        {
            var indexStateRepository = new Mock<IIndexStateRepository>();

            indexStateRepository.Setup(o => o.GetAsync())
                .Returns(() => Task.FromResult(_lastIndexState));

            indexStateRepository.Setup(o => o.SetAsync(It.IsAny<IndexState>()))
                .Returns((IndexState newState) =>
                {
                    _lastIndexState = newState;
                    return Task.CompletedTask;
                });

            _indexStateRepository = indexStateRepository.Object;
        }

        private readonly IList<IndexHistory> _indexHistories = new List<IndexHistory>();
        private void InitializeIndexHistoryRepository()
        {
            var indexHistoryRepository = new Mock<IIndexHistoryRepository>();

            indexHistoryRepository.Setup(o => o.TakeLastAsync(It.IsAny<int>(), It.IsAny<DateTime?>()))
                .Returns((int take, DateTime? from) =>
                {
                    var indexHistories = _indexHistories;

                    if (from.HasValue)
                        indexHistories = indexHistories.Where(x => x.Time > from).ToList();

                    indexHistories = indexHistories.OrderByDescending(x => x.Time).Take(take).ToList();

                    return Task.FromResult((IReadOnlyList<IndexHistory>)indexHistories);
                });

            indexHistoryRepository.Setup(o => o.InsertAsync(It.IsAny<IndexHistory>()))
                .Returns((IndexHistory newIndexHistory) =>
                {
                    _indexHistories.Add(newIndexHistory);

                    return Task.CompletedTask;
                });

            _indexHistoryRepository = indexHistoryRepository.Object;
        }

        private DateTime? _resetTimestamp;
        private void InitializeFirstStateAfterResetTimeRepository()
        {
            var indexFirstStateAfterResetTimeRepository = new Mock<IFirstStateAfterResetTimeRepository>();

            indexFirstStateAfterResetTimeRepository.Setup(o => o.GetAsync())
                .Returns(() => Task.FromResult(_resetTimestamp));

            indexFirstStateAfterResetTimeRepository.Setup(o => o.SetAsync(It.IsAny<DateTime>()))
                .Returns((DateTime timestamp) =>
                {
                    _resetTimestamp = timestamp;
                    return Task.CompletedTask;
                });

            _firstStateAfterResetTimeRepository = indexFirstStateAfterResetTimeRepository.Object;
        }

        private void InitializeTickPricesService()
        {
            var tickPricesService = new Mock<ITickPricesService>();

            tickPricesService.Setup(o => o.GetAssetPrices(It.IsAny<IReadOnlyCollection<string>>()))
                .Returns((ICollection<string> sources) =>
                {
                    IDictionary<string, IReadOnlyCollection<AssetPrice>> result = 
                        new Dictionary<string, IReadOnlyCollection<AssetPrice>>();

                    var prices = GetPrices(_step);

                    foreach (var price in prices)
                        result.Add(price.Asset, new List<AssetPrice> { new AssetPrice
                        {
                            Asset = price.Asset,
                            CrossAsset = price.CrossAsset,
                            Source = price.Source,
                            Price = price.Price
                        } });

                    return result;
                });

            tickPricesService.Setup(o => o.GetTickPrices(It.IsAny<IReadOnlyCollection<string>>()))
                .Returns((ICollection<string> sources) =>
                {
                    IDictionary<string, IReadOnlyCollection<TickPrice>> result =
                        new Dictionary<string, IReadOnlyCollection<TickPrice>>();

                    var prices = GetPrices(_step);

                    foreach (var price in prices)
                        result.Add(price.Asset, new List<TickPrice> {
                            new TickPrice(
                                price.Source,
                                price.Asset + price.CrossAsset,
                                price.Price,
                                price.Price,
                                DateTime.UtcNow
                            )});

                    return result;
                });

            _tickPricesService = tickPricesService.Object;
        }

        private void InitializeIndexHandler()
        {
            var indexHandler = new Mock<IIndexHandler>();

            indexHandler.Setup(o => o.HandleAsync(It.IsAny<IndexHistory>()));

            _indexHandler = indexHandler.Object;
        }

        private void InitializeCoinMarketCapService()
        {
            var coinMarketCapService = new Mock<ICoinMarketCapService>();

            coinMarketCapService.Setup(o => o.GetAllAsync()).Returns(() =>
            {
                var result = new List<AssetMarketCap>();

                var supplies = GetSupply(_step);

                foreach (var supply in supplies)
                {
                    result.Add(new AssetMarketCap(supply.Key, new MarketCap(0, Usd), supply.Value));
                }

                return Task.FromResult((IReadOnlyList<AssetMarketCap>)result);
            });

            _coinMarketCapService = coinMarketCapService.Object;
        }

        private void InitializeTickPricePublisher()
        {
            var tickPricePublisher = new Mock<ITickPricePublisher>();

            tickPricePublisher.Setup(o => o.Publish(It.IsAny<Contract.IndexTickPrice>()))
                .Callback((Contract.IndexTickPrice indexTickPrice) =>
                {
                    AssertPublishedIndex(indexTickPrice);
                    _step++;
                });

            _tickPricePublisher = tickPricePublisher.Object;
        }

        private void InitializeWarningRepository()
        {
            var warningRepository = new Mock<IWarningRepository>();

            warningRepository.Setup(o => o.SaveAsync(It.IsAny<Warning>()))
                .Returns(() =>
                {
                    return Task.CompletedTask;
                });

            _warningRepository = warningRepository.Object;
        }

        private void InitializeDependencies()
        {
            InitializeIndexStateRepository();

            InitializeIndexHistoryRepository();

            InitializeFirstStateAfterResetTimeRepository();

            InitializeWarningRepository();


            InitializeSettingsService();

            InitializeTickPricesService();

            InitializeCoinMarketCapService();

            InitializeTickPricePublisher();

            InitializeIndexHandler();
        }

        // act

        [Fact]
        public void Simple_Test()
        {
            InitializeDependencies();
            
            _indexCalculator = new IndexCalculator(
                LyCi,
                ShortLyCi,
                true,
                _timerInterval,
                _settingsService,
                _indexStateRepository,
                _indexHistoryRepository,
                _tickPricesService,
                _coinMarketCapService,
                _tickPricePublisher,
                _warningRepository,
                _firstStateAfterResetTimeRepository,
                _indexHandler,
                LogFactory.Create());

            _indexCalculator.Start();

            _indexCalculator.Rebuild();

            var stepsCount = GetPrices(0).Count - 1; // without header

            var counter = 0;
            while (_step != stepsCount)
            {
                counter++;
                Thread.Sleep(1000);

                if (counter > 10)
                    throw new TimeoutException("Debug IndexCalculator.CalculateThenSaveAndPublishAsync() to find exception.");
            }

            _indexCalculator.Stop();
        }

        private IDictionary<string, decimal> GetSupply(int step)
        {
            var result = new Dictionary<string, decimal>();

            var steps = SuppliesForEachStep.Split("\r\n").ToList();

            var assets = steps.First().Split("\t").Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();

            var supplies = steps.TakeLast(steps.Count - 1).ToList()[step].Split("\t").Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();

            for (var i = 0; i < supplies.Count; i++)
            {
                result[assets[i]] = decimal.Parse(supplies[i]);
            }

            return result;
        }

        private IList<IDictionary<string, decimal>> GetSupplies()
        {
            var result = new List<IDictionary<string, decimal>>();

            var steps = SuppliesForEachStep.Split("\r\n").Length - 1;

            for (var i = 0; i < steps; i++)
                result.Add(GetSupply(i));

            return result;
        }

        private IReadOnlyCollection<AssetPrice> GetPrices(int step)
        {
            var result = new List<AssetPrice>();

            var steps = IndicesValuesAndPricesForEachStep.Split("\r\n").ToList();

            var assets = steps.First().Split("\t").Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();

            var prices = steps.TakeLast(steps.Count - 1).ToList()[step].Split("\t").Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();

            for (var i = 1; i < prices.Count; i++)
            {
                var newAssetPrice = new AssetPrice
                {
                    Asset = assets[i],
                    CrossAsset = "USD",
                    Source = "fakeExchange",
                    Price = decimal.Parse(prices[i])
                };

                result.Add(newAssetPrice);
            }

            return result;
        }

        private decimal GetIndexValue(int step)
        {
            var steps = IndicesValuesAndPricesForEachStep.Split("\r\n").ToList();

            var assets = steps.First().Split("\t").Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();

            var indicesAndPrices = steps.TakeLast(steps.Count - 1).ToList()[step].Split("\t").Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();
            
            var result = decimal.Parse(indicesAndPrices[0]);

            return result;
        }


        // assert

        private void AssertPublishedIndex(Contract.IndexTickPrice indexTickPrice)
        {
            var indexValue = GetIndexValue(_step);

            var stepsCount = GetPrices(0).Count - 1; // without header

            Assert.Equal(indexValue, indexTickPrice.Ask);
            Assert.Equal(indexTickPrice.Ask, indexTickPrice.Bid);
        }
    }
}
