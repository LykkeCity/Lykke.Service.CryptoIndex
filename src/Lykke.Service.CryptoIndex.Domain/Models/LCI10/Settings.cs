using System;
using System.Collections.Generic;

namespace Lykke.Service.CryptoIndex.Domain.Models.LCI10
{
    public class Settings
    {
        /// Where the prices shall be taken from
        public IReadOnlyList<string> Sources { get; }

        /// Constituents of index (assets which we build the index from)
        public IReadOnlyList<string> Assets { get; }

        /// Interval between two index calculations
        public TimeSpan IndexCalculationInterval { get; }

        /// Interval between two weights calculations
        public TimeSpan WeigthsCalculationInterval { get; }

        public Settings(IReadOnlyList<string> sources, IReadOnlyList<string> assets, TimeSpan indexCalculationInterval, TimeSpan weigthsCalculationInterval)
        {
            Sources = sources;
            Assets = assets;
            IndexCalculationInterval = indexCalculationInterval;
            WeigthsCalculationInterval = weigthsCalculationInterval;
        }
    }
}
