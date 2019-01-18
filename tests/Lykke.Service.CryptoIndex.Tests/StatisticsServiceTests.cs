using System;
using System.Collections.Generic;
using Lykke.Service.CryptoIndex.Domain.Services;
using Xunit;

namespace Lykke.Service.CryptoIndex.Tests
{
    public class StatisticsServiceTests
    {
        [Fact]
        public void Return_Calculation_Test()
        {
            // arrange

            var previousVaue = 10;
            var nextValue = 11;

            // act

            var _return = StatisticsService.CalculateReturn(previousVaue, nextValue);

            // assert

            Assert.Equal(10, _return);
        }

        [Fact]
        public void Return_Calculation_Negative_Test()
        {
            // arrange

            var previousVaue = 10;
            var nextValue = 9;

            // act

            var _return = StatisticsService.CalculateReturn(previousVaue, nextValue);

            // assert

            Assert.Equal(-10, _return);
        }

        [Fact]
        public void Volatility_Calculation_Test()
        {
            // arrange

            var values = new SortedDictionary<DateTime, decimal>();
            values.Add(DateTime.MinValue.AddHours(1), 9);
            values.Add(DateTime.MinValue.AddHours(2), 10);
            values.Add(DateTime.MinValue.AddHours(3), 11);

            // act

            var volatility = StatisticsService.CalculateVolatility(values, TimeSpan.MinValue);

            // assert

            Assert.Equal(0.555555555555556m, volatility);
        }
    }
}
