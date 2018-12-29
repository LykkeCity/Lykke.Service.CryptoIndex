using AzureStorage;
using Lykke.Service.CryptoIndex.Domain.Repositories.Models;

namespace Lykke.Service.CryptoIndex.Domain.Repositories.Repositories
{
    public class ChartHistory30DRepository : ChartHistoryRepository, IChartHistory30DRepository
    {
        public ChartHistory30DRepository(INoSQLTableStorage<HistoryPointEntity> storage) : base(storage)
        {
        }
    }
}
