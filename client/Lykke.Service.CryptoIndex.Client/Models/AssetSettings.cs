namespace Lykke.Service.CryptoIndex.Client.Models
{
    /// <summary>
    /// Settings 
    /// </summary>
    public class AssetSettings
    {
        /// <summary>
        /// Identifier of the asset.
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// Middle price of the asset.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// True if the asset was 'frozen'.
        /// </summary>
        public bool IsDisabled { get; set; }

        /// <summary>
        /// True if the asset is automatically 'frozen'.
        /// </summary>
        public bool IsAutoDisabled { get; set; }
    }
}
