using System.Threading.Tasks;

namespace Lykke.Service.CryptoIndex.Domain.LCI10.IndexHistory
{
    public interface IIndexHistoryRepository
    {
        Task InsertAsync(IndexHistory indexHistory);
    }
}
