namespace Lykke.Service.CryptoIndex.Domain.Models
{
    public class AssetInfo
    {
        /// <summary>
        /// Identifier of the asset.
        /// </summary>
        public string AssetId { get; }

        /// <summary>
        /// Weight of the asset.
        /// </summary>
        public decimal Weight { get; }

        /// <summary>
        /// Middle price of the asset.
        /// </summary>
        public decimal Price { get; }

        /// <summary>
        /// Middle price of the asset.
        /// </summary>
        public bool IsDisabled { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="AssetInfo"/>.
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="weight"></param>
        /// <param name="price"></param>
        /// <param name="isDisabled"></param>
        public AssetInfo(string assetId, decimal weight, decimal price, bool isDisabled)
        {
            AssetId = assetId;
            Weight = weight;
            Price = price;
            IsDisabled = isDisabled;
        }
    }
}
