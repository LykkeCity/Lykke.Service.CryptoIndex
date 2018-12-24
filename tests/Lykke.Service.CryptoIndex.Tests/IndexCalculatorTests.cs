using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lykke.Logs;
using Lykke.Service.CryptoIndex.Domain.Models;
using Lykke.Service.CryptoIndex.Domain.Publishers;
using Lykke.Service.CryptoIndex.Domain.Repositories;
using Lykke.Service.CryptoIndex.Domain.Services;
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
        private ITickPricesService _tickPricesService;
        private ICoinMarketCapService _coinMarketCapService;
        private ITickPricePublisher _tickPricePublisher;
        private IWarningRepository _warningRepository;
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

        private void InitializeTickPricesService()
        {
            var tickPricesService = new Mock<ITickPricesService>();

            tickPricesService.Setup(o => o.GetPricesAsync(It.IsAny<ICollection<string>>()))
                .Returns((ICollection<string> sources) =>
                {
                    IDictionary<string, IDictionary<string, decimal>> result = 
                        new Dictionary<string, IDictionary<string, decimal>>();

                    var prices = GetPrices(_step);

                    foreach (var price in prices)
                        result.Add(price.Key, new Dictionary<string, decimal> { { "fakeExchange", price.Value} });

                    return Task.FromResult(result);
                });

            _tickPricesService = tickPricesService.Object;
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

            tickPricePublisher.Setup(o => o.Publish(It.IsAny<IndexTickPrice>()))
                .Callback((IndexTickPrice indexTickPrice) =>
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
            InitializeSettingsService();

            InitializeIndexStateRepository();

            InitializeIndexHistoryRepository();

            InitializeTickPricesService();

            InitializeCoinMarketCapService();

            InitializeTickPricePublisher();

            InitializeWarningRepository();
        }


        // act
        [Fact]
        public void Simple_Test()
        {
            InitializeDependencies();
            
            _indexCalculator = new IndexCalculator(
                LyCi,
                _timerInterval,
                _settingsService,
                _indexStateRepository,
                _indexHistoryRepository,
                _tickPricesService,
                _coinMarketCapService,
                _tickPricePublisher,
                _warningRepository,
                LogFactory.Create());

            _indexCalculator.Start();

            var stepsCount = GetPrices(0).Count - 1; // without header

            while (_step != stepsCount - 1)
            {
                Thread.Sleep(1000);
            }
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

        private IDictionary<string, decimal> GetPrices(int step)
        {
            var result = new Dictionary<string, decimal>();

            var steps = IndicesValuesAndPricesForEachStep.Split("\r\n").ToList();

            var assets = steps.First().Split("\t").Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();

            var prices = steps.TakeLast(steps.Count - 1).ToList()[step].Split("\t").Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();

            for (var i = 1; i < prices.Count; i++)
            {
                result[assets[i]] = decimal.Parse(prices[i]);
            }

            return result;
        }

        private decimal GetIndexValue(int step)
        {
            var result = -1m;

            var steps = IndicesValuesAndPricesForEachStep.Split("\r\n").ToList();

            var assets = steps.First().Split("\t").Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();

            var indicesAndPrices = steps.TakeLast(steps.Count - 1).ToList()[step].Split("\t").Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();
            
            result = decimal.Parse(indicesAndPrices[0]);

            return result;
        }


        // assert

        private void AssertPublishedIndex(IndexTickPrice indexTickPrice)
        {
            var indexValue = GetIndexValue(_step);

            var stepsCount = GetPrices(0).Count - 1; // without header

            Assert.Equal(indexValue, indexTickPrice.Ask);
            Assert.Equal(indexTickPrice.Ask, indexTickPrice.Bid);

            if (_step == stepsCount - 1)
            {
                _indexCalculator.Stop();
            }
        }
    }
}
