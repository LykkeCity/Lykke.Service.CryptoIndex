using AzureStorage;
using Lykke.Service.CryptoIndex.Domain.Repositories.Models;

namespace Lykke.Service.CryptoIndex.Domain.Repositories.Repositories
{
    public class ChartHistory5DRepository : ChartHistoryRepository, IChartHistory5DRepository
    {
        public ChartHistory5DRepository(INoSQLTableStorage<HistoryPointEntity> storage) : base(storage)
        {
        }
    }
}
