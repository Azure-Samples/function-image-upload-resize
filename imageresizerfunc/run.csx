#r "Microsoft.Azure.WebJobs.Extensions.EventGrid"
#r "Microsoft.WindowsAzure.Storage"
#r "Newtonsoft.Json"

using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Host.Bindings.Runtime;
using Microsoft.WindowsAzure.Storage; 
using Microsoft.WindowsAzure.Storage.Blob;
using ImageResizer;
using ImageResizer.ExtensionMethods;

static string storageAccountConnectionString = System.Environment.GetEnvironmentVariable("myBlobStorage_STORAGE");
static string thumbContainerName = System.Environment.GetEnvironmentVariable("myContainerName");

public static async Task Run(EventGridEvent myEvent, TraceWriter log)
{
    log.Info(myEvent.ToString());

    // Get the blobname from the event's JObject.
    var triggeredBlobUri = new Uri((string)myEvent.Data["url"]);

    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageAccountConnectionString);
    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
    
    var cloudBlob = await blobClient.GetBlobReferenceFromServerAsync(triggeredBlobUri);

    string containerName = GetContainerName(cloudBlob);
    string blobName = GetBlobName(cloudBlob);

    // Instructions to resize the blob image.
    var instructions = new Instructions
    {
        Width = 150,
        Height = 150,
        Mode = FitMode.Crop,
        Scale = ScaleMode.Both
    };

    var outputBlob = CreateOutputBlob(blobClient, blobName);

    using(MemoryStream inStream = new MemoryStream())
    {
        await RequestInputBlob(cloudBlob, inStream);
        ResetStreamPosition(inStream);

        using(MemoryStream outStream = new MemoryStream())
        {
            // Resize the image with the given instructions into the stream.
            ImageBuilder.Current.Build(new ImageJob(inStream, outStream, instructions));
            
            // Reset the stream's position to the beginning.
            ResetStreamPosition(outStream);

            // Write the stream to the new blob.
            await outputBlob.UploadFromStreamAsync(outStream);
        }
    }
}

private static async Task RequestInputBlob(ICloudBlob cloudBlob, Stream inputStream)
{
    await cloudBlob.DownloadToStreamAsync(inputStream);
}

private static ICloudBlob CreateOutputBlob(CloudBlobClient blobClient, string blobName)
{
    // Retrieve reference to a previously created container.
    CloudBlobContainer container = blobClient.GetContainerReference(thumbContainerName);

    // Create reference to a blob named "blobName".
    return container.GetBlockBlobReference(blobName);
}

private static string GetBlobName(ICloudBlob cloudBlob)
{
    return cloudBlob.Name;
}

private static string GetContainerName(ICloudBlob cloudBlob)
{
    return cloudBlob.Container.Name;
}

private static void ResetStreamPosition(Stream stream)
{
    stream.Position = 0;
}
