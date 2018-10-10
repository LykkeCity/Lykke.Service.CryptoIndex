using JetBrains.Annotations;
using Lykke.Sdk.Settings;

namespace Lykke.Service.CryptoIndex.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public CryptoIndexSettings CryptoIndexService { get; set; }
    }
}
