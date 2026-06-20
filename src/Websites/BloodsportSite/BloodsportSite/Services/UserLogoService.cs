using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace BloodsportSite.Services
{
    public class UserLogoService(BlobServiceClient blobServiceClient)
    {
        private const string ContainerName = "bs-user-logo";
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB
        private static readonly HashSet<string> AllowedContentTypes = ["image/jpeg", "image/png", "image/gif", "image/webp"];

        public async Task<string?> UploadLogoAsync(long userId, IFormFile file)
        {
            if (file.Length == 0 || file.Length > MaxFileSizeBytes)
                return null;

            var contentType = file.ContentType.ToLowerInvariant();
            if (!AllowedContentTypes.Contains(contentType))
                return null;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var blobName = $"{userId}/original{extension}";

            var container = blobServiceClient.GetBlobContainerClient(ContainerName);
            var blob = container.GetBlobClient(blobName);
            await using var stream = file.OpenReadStream();
            await blob.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = contentType } });

            return blob.Uri.ToString();
        }
    }
}
