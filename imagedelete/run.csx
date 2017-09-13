#r "Microsoft.Azure.WebJobs.Extensions.EventGrid"
#r "Microsoft.WindowsAzure.Storage"
#r "Newtonsoft.Json"

#load "../shared/StorageHelper.cs"

using Microsoft.WindowsAzure.Storage; 
using Microsoft.WindowsAzure.Storage.Blob;

using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Host.Bindings.Runtime;

public static async Task Run(EventGridEvent myEvent, TraceWriter log)
{
    log.Info(myEvent.ToString());

    // Get the blobname from the event's JObject.
    var deletedBlobUri = new Uri((string)myEvent.Data["url"]);
    var blobClient = StorageHelper.GetBlobClient();
    var deletedImage = new CloudBlob(deletedBlobUri);

    //Get a reference to the other image
	var otherImage = deletedImage.Container.Name.Equals(ImageContainerName) ? 
		await StorageHelper.GetThumbnailBlobReference(blobClient, deletedImage.Name) :
		await StorageHelper.GetImageBlobReference(blobClient, deletedImage.Name);    
	
	await otherImage.DeleteIfExistsAsync();
}
