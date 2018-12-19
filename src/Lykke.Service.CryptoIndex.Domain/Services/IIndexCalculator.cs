using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.CryptoIndex.Domain.Services
{
    public interface IIndexCalculator
    {
        IReadOnlyDictionary<string, decimal> GetAllAssetsMarketCaps();

        Task Reset();

        Task Rebuild();

        decimal GetLastValue();

        DateTime? GetLastResetTimestamp();
    }
}
