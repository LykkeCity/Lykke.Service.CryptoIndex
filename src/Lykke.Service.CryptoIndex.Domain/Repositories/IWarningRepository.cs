using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.CryptoIndex.Domain.Models;

namespace Lykke.Service.CryptoIndex.Domain.Repositories
{
    public interface IWarningRepository
    {
        Task SaveAsync(Warning warning);

        Task<IReadOnlyList<Warning>> GetAsync(DateTime from, DateTime to);
    }
}
