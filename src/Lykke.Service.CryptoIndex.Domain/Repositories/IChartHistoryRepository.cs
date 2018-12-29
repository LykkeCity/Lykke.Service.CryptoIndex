using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.CryptoIndex.Domain.Repositories
{
    public interface IChartHistoryRepository
    {
        Task InsertOrReplaceAsync(DateTime time, decimal value);

        Task<IReadOnlyDictionary<DateTime, decimal>> GetAsync(DateTime from, DateTime to);
    }
}
