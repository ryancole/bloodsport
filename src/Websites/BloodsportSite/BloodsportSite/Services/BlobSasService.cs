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

            var uri = new Uri(blobUrl);
            var segments = uri.AbsolutePath.TrimStart('/').Split('/', 2);
            if (segments.Length < 2) return null;

            var blobClient = blobServiceClient
                .GetBlobContainerClient(segments[0])
                .GetBlobClient(segments[1]);

            return blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(Expiry)).ToString();
        }
    }
}
