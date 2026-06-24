using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace BloodsportSite.Services
{
    public class BlobSasService(BlobServiceClient blobServiceClient)
    {
        private static readonly TimeSpan Expiry = TimeSpan.FromHours(1);

        public string? GetSasUrl(string? blobUrl)
        {
            if (blobUrl is null) return null;

            var parsed = new BlobUriBuilder(new Uri(blobUrl));

            var blobClient = blobServiceClient
                .GetBlobContainerClient(parsed.BlobContainerName)
                .GetBlobClient(parsed.BlobName);

            return blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(Expiry)).ToString();
        }
    }
}
