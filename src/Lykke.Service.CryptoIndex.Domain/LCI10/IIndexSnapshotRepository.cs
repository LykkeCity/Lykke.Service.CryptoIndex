using System.Threading.Tasks;

namespace Lykke.Service.CryptoIndex.Domain.LCI10
{
    public interface IIndexSnapshotRepository
    {
        Task<IndexSnapshot> GetLatestAsync();

        Task InsertAsync(IndexSnapshot indexSnapshot);
    }
}
