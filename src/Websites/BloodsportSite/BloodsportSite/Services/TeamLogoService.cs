using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace BloodsportSite.Services
{
    public class TeamLogoService(BlobServiceClient blobServiceClient)
    {
        private const string ContainerName = "bs-team-logo";
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB
        private static readonly HashSet<string> AllowedContentTypes = ["image/jpeg", "image/png", "image/gif", "image/webp"];

        public async Task<string?> UploadLogoAsync(long teamId, IFormFile file)
        {
            if (file.Length == 0 || file.Length > MaxFileSizeBytes)
                return null;

            var contentType = file.ContentType.ToLowerInvariant();
            if (!AllowedContentTypes.Contains(contentType))
                return null;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var blobName = $"{teamId}/original{extension}";

            var container = blobServiceClient.GetBlobContainerClient(ContainerName);
            await container.CreateIfNotExistsAsync(PublicAccessType.None);

            var blob = container.GetBlobClient(blobName);
            await using var stream = file.OpenReadStream();
            await blob.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = contentType } });

            return blob.Uri.ToString();
        }

        public string? GetSasUrl(string? blobUrl)
        {
            if (blobUrl is null)
                return null;

            var blobUri = new Uri(blobUrl);
            var blobName = string.Join("", blobUri.Segments[2..]);
            var container = blobServiceClient.GetBlobContainerClient(ContainerName);
            var blob = container.GetBlobClient(blobName);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = ContainerName,
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1),
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            return blob.GenerateSasUri(sasBuilder).ToString();
        }
    }
}
