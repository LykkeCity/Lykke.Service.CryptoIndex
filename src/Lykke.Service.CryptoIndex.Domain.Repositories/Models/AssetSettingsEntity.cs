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

        /// <inheritdoc />
        public AssetSettingsEntity(string assetId, decimal price, bool isDisabled)
        {
            AssetId = assetId;
            Price = price;
            IsDisabled = isDisabled;
        }
    }
}
