namespace Lykke.Service.CryptoIndex.Domain.Repositories.Models
{
    public class AssetSettingsEntity
    {
        /// <summary>
        /// Identifier of the asset.
        /// </summary>
        public string AssetId { get; }

        /// <summary>
        /// Middle price of the asset.
        /// </summary>
        public decimal Price { get; }

        /// <summary>
        /// True if the asset was 'frozen'.
        /// </summary>
        public bool IsDisabled { get; }

        /// <summary>
        /// True if the asset is automatically 'frozen'.
        /// </summary>
        public bool IsAutoDisabled { get; }

        /// <inheritdoc />
        public AssetSettingsEntity(string assetId, decimal price, bool isDisabled, bool isAutoDisabled)
        {
            AssetId = assetId;
            Price = price;
            IsDisabled = isDisabled;
            IsAutoDisabled = isAutoDisabled;
        }
    }
}
