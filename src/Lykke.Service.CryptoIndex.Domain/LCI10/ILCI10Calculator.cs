﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.CryptoIndex.Domain.LCI10
{
    public interface ILCI10Calculator
    {
        Task<IReadOnlyDictionary<string, decimal>> GetAssetMarketCapAsync();
    }
}
