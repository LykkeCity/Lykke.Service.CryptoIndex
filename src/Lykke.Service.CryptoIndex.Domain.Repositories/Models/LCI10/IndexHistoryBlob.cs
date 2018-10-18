using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Lykke.Service.CryptoIndex.Domain.Repositories.Models.LCI10
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class IndexHistoryBlob
    {
        public IDictionary<string, IDictionary<string, decimal>> Prices { get; set; }

        public DateTime Time { get; set; }

        public override string ToString()
        {
            return $"{Time}";
        }
    }
}
