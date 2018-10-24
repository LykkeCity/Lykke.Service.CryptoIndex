using JetBrains.Annotations;

namespace Lykke.Service.CryptoIndex.Client.Models.LCI10
{
    /// <summary>
    /// Current and previous values
    /// </summary>
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class TwoTickPrices
    {
        /// <summary>
        /// Current value of index
        /// </summary>
        public decimal Current { get; set; }

        /// <summary>
        /// Previous value of index
        /// </summary>
        public decimal Previous { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public TwoTickPrices(decimal current, decimal previous)
        {
            Current = current;
            Previous = previous;
        }
    }
}
