using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.CryptoIndex.Domain.Models.LCI10;

namespace Lykke.Service.CryptoIndex.Domain.Repositories.LCI10
{
    public interface IIndexHistoryRepository
    {
        Task InsertAsync(IndexHistory domain);

        Task<IndexHistory> GetAsync(DateTime timestamp);

        Task<IReadOnlyList<IndexHistory>> GetAsync(DateTime from, DateTime to);

        Task<IReadOnlyList<DateTime>> GetTimestampsAsync(DateTime from, DateTime to);

        Task Clear();
    }
}
