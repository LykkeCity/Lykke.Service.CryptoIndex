namespace Lykke.Service.CryptoIndex.Client.Models
{
    /// <summary>
    /// Represents an ignored asset
    /// </summary>
    public class IgnoredAsset
    {
        /// <summary>
        /// Asset
        /// </summary>
        public string Asset { get; set; }

        /// <summary>
        /// Reason to ignore
        /// </summary>
        public string ReasonToIgnore { get; set; }

        ///<inheritdoc />
        public override string ToString()
        {
            return $"{Asset}, {ReasonToIgnore}";
        }
    }
}
