using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.CryptoIndex.Domain.LCI10.IndexHistory
{
    public interface IIndexHistoryRepository
    {
        Task<IndexHistory> GetLatestAsync();

        Task<IReadOnlyList<IndexHistory>> GetAsync(DateTime from, DateTime to, int limit);

        Task InsertAsync(IndexHistory indexHistory);
    }
}
