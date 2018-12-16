using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.CryptoIndex.Domain.Models;

namespace Lykke.Service.CryptoIndex.Domain.Services
{
    public interface ICoinMarketCapService : IDisposable
    {
        Task<IReadOnlyList<AssetMarketCap>> GetAllAsync();
    }
}
