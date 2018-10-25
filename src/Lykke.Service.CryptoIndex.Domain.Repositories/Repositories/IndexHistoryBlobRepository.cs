using System;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Lykke.Service.CryptoIndex.Domain.Repositories.Models;

namespace Lykke.Service.CryptoIndex.Domain.Repositories.Repositories
{
    public class IndexHistoryBlobRepository : BaseBlobRepository
    {
        private const string ContainerName = "index-history";

        public IndexHistoryBlobRepository(IBlobStorage storage) : base(storage, ContainerName)
        {
        }

        public Task SaveAsync(IndexHistoryBlob indexHistory)
        {
            return SaveBlobAsync(GetBlobName(indexHistory.Time), indexHistory.ToJson());
        }

        public async Task<IndexHistoryBlob> GetAsync(DateTime dateTime)
        {
            var blobName = GetBlobName(dateTime);
            if (!await BlobExistsAsync(blobName))
                return null;

            return (await GetBlobStringAsync(blobName)).DeserializeJson<IndexHistoryBlob>();
        }

        public async Task DeleteIfExistsAsync(DateTime dateTime)
        {
            var blobName = GetBlobName(dateTime);
            if (await BlobExistsAsync(blobName))
                await DeleteBlobAsync(blobName);
        }

        public static string GetBlobName(DateTime dateTime)
            => $"{dateTime.ToIsoDateTime()}";
    }
}
