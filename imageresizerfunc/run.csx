#r "Microsoft.Azure.WebJobs.Extensions.EventGrid"
#r "Microsoft.WindowsAzure.Storage"
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Host.Bindings.Runtime;
using Microsoft.WindowsAzure.Storage; 
using Microsoft.WindowsAzure.Storage.Blob;
using ImageResizer;
using ImageResizer.ExtensionMethods;

public static async Task Run(EventGridEvent e, Stream inputBlob, TraceWriter log)
{
    log.Info(e == null? "null event" : e.ToString());

    var instructions = new Instructions
    {
        Width = 150,
        Height = 150,
        Mode = FitMode.Crop,
        Scale = ScaleMode.Both
    };    

    // Get the blobname from the event
    string blobname = e.Subject.Remove(0, e.Subject.LastIndexOf('/')+1);

    // Retrieve storage account from connection string.
    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
        System.Environment.GetEnvironmentVariable("zhstore2_STORAGE"));

    // Create the blob client.
    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

    // Retrieve reference to a previously created container.
    CloudBlobContainer container = blobClient.GetContainerReference("thumbs");

    // Retrieve reference to a blob named "myblob".
    CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobname);

    using(MemoryStream myStream = new MemoryStream())
    {  
        // Resize the image with the given instructions into the stream
        ImageBuilder.Current.Build(new ImageJob(inputBlob, myStream, instructions));
        
        // Reset the stream's position to the beginning
        myStream.Position = 0;

        // Write the stream to the new blob
        await blockBlob.UploadFromStreamAsync(myStream);
    }
}
