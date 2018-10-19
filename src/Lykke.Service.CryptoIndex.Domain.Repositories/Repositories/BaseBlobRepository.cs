using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AzureStorage;

namespace Lykke.Service.CryptoIndex.Domain.Repositories.Repositories
{
    public abstract class BaseBlobRepository
    {
        private string _container;
        protected readonly IBlobStorage Storage;

        protected BaseBlobRepository(IBlobStorage storage, string container)
        {
            _container = container;
            Storage = storage;

            storage.CreateContainerIfNotExistsAsync(_container).GetAwaiter().GetResult();
        }

        protected async Task SaveBlobAsync(string blobKey, byte[] blobData)
        {
            //TODO: additional request can be bad for performance, need to change AzureBlobStorage -> createIfNotExists
            if (await BlobExistsAsync(blobKey))
                throw new InvalidOperationException($"Blob is already existed, id: {blobKey}");

            await Storage.SaveBlobAsync(_container, blobKey, blobData);
        }

        protected async Task SaveBlobAsync(string blobKey, string blobString)
        {
            //TODO: additional request can be bad for performance, need to change AzureBlobStorage -> createIfNotExists
            if (await BlobExistsAsync(blobKey))
                throw new InvalidOperationException($"Blob is already existed, id: {blobKey}");

            await Storage.SaveBlobAsync(_container, blobKey, Encoding.UTF8.GetBytes(blobString));
        }

        protected async Task<bool> BlobExistsAsync(string blobKey)
        {
            return await Storage.HasBlobAsync(_container, blobKey);
        }

        protected async Task<byte[]> GetBlobAsync(string blobKey)
        {
            var stream = await Storage.GetAsync(_container, blobKey);
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        protected async Task<string> GetBlobStringAsync(string blobKey)
        {
            return await Storage.GetAsTextAsync(_container, blobKey);
        }

        protected async Task DeleteBlobAsync(string blobKey)
        {
            await Storage.DelBlobAsync(_container, blobKey);
        }

        protected async Task Clear()
        {
            await Storage.DeleteBlobsByPrefixAsync(_container, _container);
            await Storage.CreateContainerIfNotExistsAsync(_container);
        }
    }
}
