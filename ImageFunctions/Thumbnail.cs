// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}

// Learn how to locally debug an Event Grid-triggered function:
//    https://aka.ms/AA30pjh

// Use for local testing:
//   https://{ID}.ngrok.io/runtime/webhooks/EventGrid?functionName=Thumbnail

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Libwebp.Net;
using Libwebp.Net.utility;
using Libwebp.Standard;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Image = SixLabors.ImageSharp.Image;

namespace ImageFunctions
{
    public static class Thumbnail
    {
        private static readonly string BLOB_STORAGE_CONNECTION_STRING = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        private static readonly string THUMBNAIL_POSTFIX_FILENAME = "-thumbnail-";
        private static readonly string SUPPORTED_IMAGE_EXTENSIONS = "gif|png|jpe?g";
        private static readonly bool IS_WEBP = Convert.ToBoolean(Environment.GetEnvironmentVariable("WEBP_SUPPORT"));
        private static readonly string WEBP_EXTENSION  = ".webp";


        private static string GetBlobNameFromUrl(string bloblUrl)
        {
            var uri = new Uri(bloblUrl);
            var blobClient = new BlobClient(uri);
            return blobClient.Name;
        }

        private static IImageEncoder GetEncoder(string extension)
        {
            IImageEncoder encoder = null;

            extension = extension.Replace(".", "");

            var isSupported = Regex.IsMatch(extension, SUPPORTED_IMAGE_EXTENSIONS, RegexOptions.IgnoreCase);

            if (isSupported)
            {
                switch (extension.ToLower())
                {
                    case "png":
                        encoder = new PngEncoder();
                        break;
                    case "jpg":
                        encoder = new JpegEncoder();
                        break;
                    case "jpeg":
                        encoder = new JpegEncoder();
                        break;
                    case "gif":
                        encoder = new GifEncoder();
                        break;
                    default:
                        break;
                }
            }

            return encoder;
        }

        private static async Task<Stream> GetWebp(string filename, MemoryStream input)
        {
            var webpFilename = filename.Split('/').Last();
            var config = new WebpConfigurationBuilder()
                               .Preset(Preset.PHOTO)
                               .Output(webpFilename)
                               .Build();

            var webpEncoder = new WebpEncoder(config);
            var webpOutput = await webpEncoder.EncodeAsync(input, webpFilename);
            webpOutput.Position = 0;
            input.Position = 0;


            return webpOutput;
        }

        [FunctionName("Thumbnail")]
        public static Task Run(
            [EventGridTrigger]EventGridEvent eventGridEvent,
            [Blob("{data.url}", FileAccess.Read)] Stream input,
            ILogger log)
        {
            try
            {
                if (input != null)
                {
                    var createdEvent = ((JObject)eventGridEvent.Data).ToObject<StorageBlobCreatedEventData>();
                    var originExtension = Path.GetExtension(createdEvent.Url);
                    var extension = IS_WEBP ? WEBP_EXTENSION : originExtension;
                    var encoder = GetEncoder(originExtension);

                    if (encoder != null)
                    {
                        var thumbnailWidths = Environment.GetEnvironmentVariable("THUMBNAIL_WIDTHS").Trim().Split(',').Select((width) => Convert.ToInt32(width)).ToList();
                        var thumbContainerName = Environment.GetEnvironmentVariable("THUMBNAIL_CONTAINER_NAME");
                        var blobServiceClient = new BlobServiceClient(BLOB_STORAGE_CONNECTION_STRING);
                        var blobContainerClient = blobServiceClient.GetBlobContainerClient(thumbContainerName);
                        var originBlobName = GetBlobNameFromUrl(createdEvent.Url);
                        var blobHttpHeader = new BlobHttpHeaders { ContentType = "image/" + extension.Replace(".", "") };

                        using (Image<Rgba32> originImage = Image.Load<Rgba32>(input))
                        {
                            thumbnailWidths.ForEach(async (width) =>
                            {
                                var image = originImage.CloneAs<Rgba32>();
                                var blobName = originBlobName.Replace(originExtension, $"{THUMBNAIL_POSTFIX_FILENAME}{width}{extension}");

                                using (var output = new MemoryStream())
                                {
                                    if (width <= image.Width)
                                    {
                                        var divisor = image.Width / width;
                                        var height = Convert.ToInt32(Math.Round((decimal)(image.Height / divisor)));

                                        image.Mutate(x => x.Resize(width, height));
                                    }

                                    image.Save(output, encoder);
                                    output.Position = 0;

                                    var stream = IS_WEBP ? await GetWebp(blobName, output) : output;

                                    await blobContainerClient.GetBlobClient(blobName).UploadAsync(stream, new BlobUploadOptions { HttpHeaders = blobHttpHeader });
                                }
                            });
                        }
                    }
                    else
                    {
                        log.LogInformation($"No encoder support for: {createdEvent.Url}");
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogInformation(ex.Message);
                throw;
            }

            return Task.CompletedTask;
        }
    }
}
