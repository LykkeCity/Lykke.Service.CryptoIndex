using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.CryptoIndex.Domain.Models;

namespace Lykke.Service.CryptoIndex.Domain.Services
{
    public interface IIndexCalculator
    {
        IReadOnlyDictionary<string, decimal> GetAllAssetsMarketCaps();

        Task ResetAsync();

        void Rebuild();

        DateTime? GetLastReset();

        IndexHistory GetLastIndexHistory();

        Task CheckForNewAssets();
    }
}
