using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Logs;
using Lykke.Service.CryptoIndex.Domain.Models;
using Lykke.Service.CryptoIndex.Domain.Services;
using Xunit;

namespace Lykke.Service.CryptoIndex.Tests
{
    public class UtilsTests
    {
        [Fact]
        public void Middle_Prices_Calculation()
        {
            // arrange

            const string asset = "BTC";
            var assetExchangesPrices = new List<AssetPrice>();
            assetExchangesPrices.Add(new AssetPrice { Price = 100 });
            assetExchangesPrices.Add(new AssetPrice { Price = 50 });

            // act

            var middlePrice = Utils.GetMiddlePrice(asset, assetExchangesPrices);

            // assert

            Assert.Equal(75, middlePrice);
        }

        [Fact]
        public void Middle_Prices_Calculation_Without_Extremes()
        {
            // arrange

            const string asset = "BTC";
            var assetExchangesPrices = new List<AssetPrice>();
            assetExchangesPrices.Add(new AssetPrice { Price = 10000 });
            assetExchangesPrices.Add(new AssetPrice { Price = 50 });
            assetExchangesPrices.Add(new AssetPrice { Price = 45 });

            // act

            var middlePrice = Utils.GetMiddlePrice(asset, assetExchangesPrices);

            // assert

            Assert.Equal(50, middlePrice);
        }

        [Fact]
        public void Middle_Prices_Calculation_Without_Extremes2()
        {
            // arrange

            const string asset = "BTC";
            var assetExchangesPrices = new List<AssetPrice>();
            assetExchangesPrices.Add(new AssetPrice { Price = 10000 });
            assetExchangesPrices.Add(new AssetPrice { Price = 55 });
            assetExchangesPrices.Add(new AssetPrice { Price = 50 });
            assetExchangesPrices.Add(new AssetPrice { Price = 45 });
            assetExchangesPrices.Add(new AssetPrice { Price = 10 });

            // act

            var middlePrice = Utils.GetMiddlePrice(asset, assetExchangesPrices);

            // assert

            Assert.Equal(50, middlePrice);
        }

        [Fact]
        public void Middle_Prices_Throw_If_No_Prices()
        {
            // arrange

            const string asset = "BTC";
            var assetExchangesPrices = new List<AssetPrice>();

            // act

            var ex1 = Assert.Throws<ArgumentOutOfRangeException>(() => Utils.GetMiddlePrice(asset, null));
            var ex2 = Assert.Throws<ArgumentOutOfRangeException>(() => Utils.GetMiddlePrice(asset, assetExchangesPrices));

            // assert

            Assert.Equal(ex1.Message, ex2.Message);
            Assert.Contains("Asset 'BTC' doesn't have any prices.", ex1.Message);
        }


        [Fact]
        public void Get_Previous_Price_Without_It_In_IndexState()
        {
            // arrange

            const string asset = "BTC";
            const decimal currentMiddlePrice = 100;

            // act

            var result = Utils.GetPreviousMiddlePrice(asset, null, currentMiddlePrice);

            // assert

            Assert.Equal(currentMiddlePrice, result);
        }

        [Fact]
        public void Get_Previous_Price()
        {
            // arrange

            const string asset = "BTC";
            const decimal currentMiddlePrice = 0;
            var middlePrices = new Dictionary<string, decimal>();
            middlePrices.Add(asset, 50);
            var indexState = new IndexState(1000, middlePrices);

            // act

            var result = Utils.GetPreviousMiddlePrice(asset, indexState, currentMiddlePrice);

            // assert

            Assert.Equal(50, result);
        }


        [Fact]
        public void Get_New_Assets()
        {
            // arrange

            var whiteList = new List<string> { "BTC", "ETH", "EOS", "LTC", "BCH", "TRX" };

            var ignored = new List<string> { "XRP", "BNB", "USDT" };

            var whiteAndIgnoredAssets = whiteList.ToList();
            whiteAndIgnoredAssets.AddRange(ignored);

            var realAbsentAsset = new List<string> { "XLM", "ADA" };

            var allMarketCapAssets = new List<string> { "BTC", "ETH", "EOS", "LTC", "BCH", "BNB", "USDT", "XLM", "ADA", "TRX", "BSV" };

            var allMarketCaps = allMarketCapAssets.Select(x => new AssetMarketCap(x, new MarketCap(0, "USD"), 0)).ToList();

            // act

            var absentAssets = Utils.GetNewAssets(whiteAndIgnoredAssets, allMarketCaps, LogFactory.Create().CreateLog(this));

            // assert

            Assert.Equal(realAbsentAsset, absentAssets);
        }
    }
}
