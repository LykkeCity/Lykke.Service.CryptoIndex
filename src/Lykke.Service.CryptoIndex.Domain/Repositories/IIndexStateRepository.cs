using System.Threading.Tasks;
using Lykke.Service.CryptoIndex.Domain.Models;

namespace Lykke.Service.CryptoIndex.Domain.Repositories
{
    public interface IIndexStateRepository
    {
        Task SetAsync(IndexState indexState);

        Task<IndexState> GetAsync();

        Task Clear();
    }
}
