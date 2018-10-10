using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.CryptoIndex.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class CryptoIndexSettings
    {
        public DbSettings Db { get; set; }
    }
}
