using System;
using JetBrains.Annotations;
using Lykke.AzureStorage.Tables;

namespace Lykke.Service.CryptoIndex.Domain.Repositories.Models.LCI10
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class WarningEntity : AzureTableEntity
    {
        public string Message { get; set; }

        public DateTime Time { get; set; }
    }
}
