﻿using System.Collections.Generic;
using JetBrains.Annotations;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;

namespace Lykke.Service.CryptoIndex.Domain.Repositories.LCI10.IndexState
{
    // TODO: remove and use history
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class IndexStateEntity : AzureTableEntity
    {
        public decimal Value { get; set; }

        [JsonValueSerializer]
        public IDictionary<string, decimal> MiddlePrices { get; set; }
    }
}
