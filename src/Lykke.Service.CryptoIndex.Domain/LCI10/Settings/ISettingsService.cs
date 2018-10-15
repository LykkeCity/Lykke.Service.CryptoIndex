using System.Threading.Tasks;

namespace Lykke.Service.CryptoIndex.Domain.LCI10.Settings
{
    public interface ISettingsService
    {
        Task<Settings> GetAsync();

        Task SetAsync(Settings settings);
    }
}
