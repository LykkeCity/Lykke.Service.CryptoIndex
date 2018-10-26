using System.Threading.Tasks;
using Lykke.Service.CryptoIndex.Domain.Models;

namespace Lykke.Service.CryptoIndex.Domain.Repositories
{
    public interface ISettingsRepository
    {
        Task<Settings> GetAsync();

        Task InsertOrReplaceAsync(Settings settings);
    }
}
