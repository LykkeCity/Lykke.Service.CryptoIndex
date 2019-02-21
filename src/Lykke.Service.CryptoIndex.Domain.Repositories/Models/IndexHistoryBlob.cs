using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Lykke.Service.CryptoIndex.Domain.Repositories.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class IndexHistoryBlob
    {
        private const string Usd = "USD";

        /// <summary>
        /// Assets USD prices
        /// </summary>
        [Obsolete("Used for existed data only, use TickPrices or AssetPrices instead.")]
        public IDictionary<string, IDictionary<string, decimal>> Prices { get; set; }
            = new Dictionary<string, IDictionary<string, decimal>>();

        /// <summary>
        /// Raw tick prices
        /// </summary>
        public IList<TickPriceEntity> TickPrices { get; set; }
            = new List<TickPriceEntity>();

        /// <summary>
        /// Usd and cross prices
        /// </summary>
        public IList<AssetPriceEntity> AssetPrices { get; set; }
            = new List<AssetPriceEntity>();

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Time { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Time}";
        }
    }
}
