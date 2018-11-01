using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.CryptoIndex.Domain.Models;

namespace Lykke.Service.CryptoIndex.Domain.Repositories
{
    public interface IIndexHistoryRepository
    {
        Task InsertAsync(IndexHistory domain);

        Task<IndexHistory> GetAsync(DateTime timestamp);

        Task<IReadOnlyList<IndexHistory>> GetAsync(DateTime from, DateTime to);

        Task<IReadOnlyList<DateTime>> GetTimestampsAsync(DateTime from, DateTime to);

        Task<IReadOnlyList<(DateTime, decimal)>> GetUpToDateAsync(DateTime to, int limit);

        Task<IReadOnlyList<IndexHistory>> TakeLastAsync(int count, DateTime? from = null);
    }
}
