using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.CryptoIndex.Domain.Models.LCI10;

namespace Lykke.Service.CryptoIndex.Domain.Repositories.LCI10
{
    public interface IWarningRepository
    {
        Task SaveAsync(Warning warning);

        Task<IReadOnlyList<Warning>> GetAsync(DateTime from, DateTime to);
    }
}
