namespace Lykke.Service.CryptoIndex.Domain.Repositories.Models
{
    /// <summary>
    /// Represents an ignored asset
    /// </summary>
    public class IgnoredAssetEntity
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
