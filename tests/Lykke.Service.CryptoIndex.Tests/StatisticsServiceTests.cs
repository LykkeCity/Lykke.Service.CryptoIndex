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
            var _return = StatisticsService.CalculateReturn(10, 11);

            Assert.Equal(10, _return);
        }

        [Fact]
        public void Return_Calculation_Negative_Test()
        {
            var _return = StatisticsService.CalculateReturn(10, 9);

            Assert.Equal(-10, _return);
        }

        [Fact]
        public void Volatility_Calculation_Test()
        {
            var values = new SortedDictionary<DateTime, decimal>();
            values.Add(DateTime.MinValue.AddHours(1), 9);
            values.Add(DateTime.MinValue.AddHours(2), 10);
            values.Add(DateTime.MinValue.AddHours(3), 11);

            var volatility = StatisticsService.CalculateVolatility(values, TimeSpan.MinValue);

            Assert.Equal(0.555555555555556m, volatility);
        }
    }
}
