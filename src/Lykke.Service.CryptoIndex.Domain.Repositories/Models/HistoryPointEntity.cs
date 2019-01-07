using System;
using JetBrains.Annotations;
using Lykke.AzureStorage.Tables;

namespace Lykke.Service.CryptoIndex.Domain.Repositories.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class HistoryPointEntity : AzureTableEntity
    {
        public decimal Value { get; set; }

        public DateTime Time { get; set; }

        public HistoryPointEntity()
        {
        }

        public HistoryPointEntity(DateTime time, decimal value)
        {
            Value = value;
            Time = time;
        }
    }
}
