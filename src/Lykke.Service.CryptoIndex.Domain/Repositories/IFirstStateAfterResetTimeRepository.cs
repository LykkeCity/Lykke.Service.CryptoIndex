using System;
using System.Threading.Tasks;

namespace Lykke.Service.CryptoIndex.Domain.Repositories
{
    public interface IFirstStateAfterResetTimeRepository
    {
        Task SetAsync(DateTime dateTime);

        Task<DateTime?> GetAsync();
    }
}
