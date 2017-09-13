#r "Microsoft.Azure.WebJobs.Extensions.EventGrid"
#r "Newtonsoft.Json"
#r "Microsoft.WindowsAzure.Storage"

#load "../Shared/StorageHelper.cs"
#load "../Shared/StreamHelper.cs"

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Host.Bindings.Runtime;
using ImageResizer;
using ImageResizer.ExtensionMethods;

public static async Task Run(EventGridEvent myEvent, TraceWriter log)
{
    log.Info(myEvent.ToString());

    // Get the blobname from the event's JObject.
    var blobUri = new Uri((string)myEvent.Data["url"]);

    var blobClient = StorageHelper.GetBlobClient();
    var cloudBlob = await StorageHelper.GetBlobReference(blobClient, blobUri);
    string blobName = StorageHelper.GetBlobName(cloudBlob);

    // Instructions to resize the blob image.
    var instructions = new Instructions
    {
        Width = 150,
        Height = 150,
        Mode = FitMode.Crop,
        Scale = ScaleMode.Both
    };
    var outputBlob = await StorageHelper.GetThumbnailBlobReference(blobClient, blobName);

    using (MemoryStream inStream = new MemoryStream())
    {
        await StorageHelper.RequestInputBlob(cloudBlob, inStream);
        await StreamHelper.ResetStreamPosition(inStream);

        using (MemoryStream outStream = new MemoryStream())
        {
            // Resize the image with the given instructions into the stream.
            ImageBuilder.Current.Build(new ImageJob(inStream, outStream, instructions));

            // Reset the stream's position to the beginning.
            await StreamHelper.ResetStreamPosition(outStream);

            // Write the stream to the new blob.
            await outputBlob.UploadFromStreamAsync(outStream);
        }
    }
}