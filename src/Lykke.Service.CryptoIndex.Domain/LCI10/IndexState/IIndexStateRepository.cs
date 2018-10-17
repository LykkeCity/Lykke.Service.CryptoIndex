using System.Threading.Tasks;

namespace Lykke.Service.CryptoIndex.Domain.LCI10.IndexState
{
    public interface IIndexStateRepository
    {
        Task SetAsync(IndexState indexState);

        Task<IndexState> GetAsync();
    }
}
