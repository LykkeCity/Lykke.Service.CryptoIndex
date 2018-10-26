using System.Threading.Tasks;
using Lykke.Service.CryptoIndex.Domain.Models;

namespace Lykke.Service.CryptoIndex.Domain.Services
{
    public interface ISettingsService
    {
        Task<Settings> GetAsync();

        Task SetAsync(Settings settings);
    }
}
