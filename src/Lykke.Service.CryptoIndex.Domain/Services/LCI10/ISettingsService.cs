using System.Threading.Tasks;
using Lykke.Service.CryptoIndex.Domain.Models.LCI10;

namespace Lykke.Service.CryptoIndex.Domain.Services.LCI10
{
    public interface ISettingsService
    {
        Task<Settings> GetAsync();

        Task SetAsync(Settings settings);
    }
}
