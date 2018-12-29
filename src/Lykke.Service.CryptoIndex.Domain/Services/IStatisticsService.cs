using System;
using System.Collections.Generic;
using Lykke.Service.CryptoIndex.Domain.Models;

namespace Lykke.Service.CryptoIndex.Domain.Services
{
    public interface IStatisticsService
    {
        IDictionary<DateTime, decimal> GetIndexHistory24H();

        IDictionary<DateTime, decimal> GetIndexHistory5D();

        IDictionary<DateTime, decimal> GetIndexHistory30D();

        KeyNumbers GetKeyNumbers();
    }
}
