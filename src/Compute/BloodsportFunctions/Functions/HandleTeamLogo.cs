using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace BloodsportFunctions.Functions;

public class HandleTeamLogo(ILogger<HandleTeamLogo> logger, BlobServiceClient blobServiceClient)
{
    private const string Container = "bs-team-logo";
    private const int FlagWidth = 28;
    private const int FlagHeight = 18;

    [Function(nameof(HandleTeamLogo))]
    public async Task Run(
        [BlobTrigger($"{Container}/{{folder}}/{{filename}}", Connection = "AzureWebJobsStorage")] Stream stream,
        string folder,
        string filename)
    {
        // Only process the original upload; ignore flag thumbnails we write ourselves.
        if (!Path.GetFileNameWithoutExtension(filename).Equals("original", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogInformation("Skipping non-original blob: {folder}/{filename}", folder, filename);
            return;
        }

        logger.LogInformation("Generating flag thumbnail for: {folder}/{filename}", folder, filename);

        var extension = Path.GetExtension(filename).ToLowerInvariant();
        var flagBlobName = $"{folder}/flag{extension}";

        IImageEncoder encoder = extension switch
        {
            ".png"  => new PngEncoder(),
            ".gif"  => new GifEncoder(),
            ".webp" => new WebpEncoder(),
            _       => new JpegEncoder { Quality = 90 },
        };

        var contentType = extension switch
        {
            ".png"  => "image/png",
            ".gif"  => "image/gif",
            ".webp" => "image/webp",
            _       => "image/jpeg",
        };

        using var image = await Image.LoadAsync(stream);

        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(FlagWidth, FlagHeight),
            Mode = ResizeMode.Crop,
            Position = AnchorPositionMode.Center,
        }));

        var container = blobServiceClient.GetBlobContainerClient(Container);
        var blob = container.GetBlobClient(flagBlobName);

        using var output = new MemoryStream();
        await image.SaveAsync(output, encoder);
        output.Position = 0;

        await blob.UploadAsync(output, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        });

        logger.LogInformation("Saved {width}x{height} flag thumbnail as {name}", FlagWidth, FlagHeight, flagBlobName);
    }
}
