using System;
using System.Collections.Generic;

namespace Lykke.Service.CryptoIndex.Domain.LCI10
{
    public class Settings
    {
        /// <summary>
        /// Where the prices shall be taken from
        /// </summary>
        public IReadOnlyList<string> Sources { get; }

        /// <summary>
        /// Constituents of index (assets which we build the index from)
        /// </summary>
        public IReadOnlyList<string> Assets { get; }

        /// <summary>
        /// Interval between two index calculations
        /// </summary>
        public TimeSpan IndexCalculationInterval { get; }

        /// <summary>
        /// Interval between two weights calculations
        /// </summary>
        public TimeSpan WeightsCalculationInterval { get; }

        private Settings(IReadOnlyList<string> sources, IReadOnlyList<string> assets, TimeSpan indexCalculationInterval, TimeSpan weightsCalculationInterval)
        {
            Sources = sources;
            Assets = assets;
            IndexCalculationInterval = indexCalculationInterval;
            WeightsCalculationInterval = weightsCalculationInterval;
        }

        public static Settings Create(IReadOnlyList<string> sources, IReadOnlyList<string> assets, TimeSpan indexCalculationInterval, TimeSpan weigthsCalculationInterval)
        {
            return new Settings(sources, assets, indexCalculationInterval, weigthsCalculationInterval);
        }
    }
}
