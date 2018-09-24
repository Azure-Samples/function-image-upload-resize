#r "Microsoft.Azure.WebJobs.Extensions.EventGrid"
#r "Microsoft.WindowsAzure.Storage"
#r "Newtonsoft.Json"
#r "ImageResizer"

using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Host.Bindings.Runtime;
using Microsoft.WindowsAzure.Storage; 
using Microsoft.WindowsAzure.Storage.Blob;
using ImageResizer;
using ImageResizer.ExtensionMethods;

static string storageAccountConnectionString = System.Environment.GetEnvironmentVariable("myBlobStorage_STORAGE");
static string thumbContainerName = System.Environment.GetEnvironmentVariable("myContainerName");

public static async Task Run(EventGridEvent myEvent, Stream inputBlob, TraceWriter log)
{
    log.Info(myEvent.ToString());

    // Instructions to resize the blob image.
    var instructions = new Instructions
    {
        Width = 150,
        Height = 150,
        Mode = FitMode.Crop,
        Scale = ScaleMode.Both
    };    

    // Get the blobname from the event's JObject.
    string blobName = GetBlobNameFromUrl((string)myEvent.Data["url"]);

    // Retrieve storage account from connection string.
    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageAccountConnectionString);

    // Create the blob client.
    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

    // Retrieve reference to a previously created container.
    CloudBlobContainer container = blobClient.GetContainerReference(thumbContainerName);

    // Create reference to a blob named "blobName".
    CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

    using(MemoryStream myStream = new MemoryStream())
    {  
        // Resize the image with the given instructions into the stream.
        ImageBuilder.Current.Build(new ImageJob(inputBlob, myStream, instructions));
        
        // Reset the stream's position to the beginning.
        myStream.Position = 0;

        // Write the stream to the new blob.
        await blockBlob.UploadFromStreamAsync(myStream);
    }
}
private static string GetBlobNameFromUrl(string bloblUrl)
{
    var myUri = new Uri(bloblUrl);
    var myCloudBlob = new CloudBlob(myUri);
    return myCloudBlob.Name;
}
