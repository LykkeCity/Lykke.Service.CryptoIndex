using System.Threading.Tasks;
using Lykke.Service.CryptoIndex.Domain.Models.LCI10;

namespace Lykke.Service.CryptoIndex.Domain.Repositories.LCI10
{
    public interface ISettingsRepository
    {
        Task<Settings> GetAsync();

        Task InsertOrReplaceAsync(Settings settings);
    }
}
