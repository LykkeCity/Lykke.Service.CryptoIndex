using System.Threading.Tasks;
using Lykke.Service.CryptoIndex.Domain.Models.LCI10;

namespace Lykke.Service.CryptoIndex.Domain.Repositories.LCI10
{
    public interface IIndexStateRepository
    {
        Task SetAsync(IndexState indexState);

        Task<IndexState> GetAsync();

        Task Clear();
    }
}
