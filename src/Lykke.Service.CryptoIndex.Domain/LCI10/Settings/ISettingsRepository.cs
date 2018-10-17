using System.Threading.Tasks;

namespace Lykke.Service.CryptoIndex.Domain.LCI10.Settings
{
    public interface ISettingsRepository
    {
        Task<Settings> GetAsync();

        Task InsertOrReplaceAsync(Settings settings);
    }
}
