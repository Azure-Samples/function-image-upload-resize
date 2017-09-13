#r "Microsoft.WindowsAzure.Storage"

using Microsoft.WindowsAzure.Storage; 
using Microsoft.WindowsAzure.Storage.Blob;

static string StorageAccountConnectionString = System.Environment.GetEnvironmentVariable("myBlobStorage_STORAGE");
static string ImageContainerName = System.Environment.GetEnvironmentVariable("myImageContainerName");
static string ThumbContainerName = System.Environment.GetEnvironmentVariable("myThumbContainerName");


public static class StorageHelper  
{
	public static CloudBlobClient GetBlobClient()
	{
		CloudStorageAccount storageAccount = CloudStorageAccount.Parse(StorageAccountConnectionString);
		return storageAccount.CreateCloudBlobClient();
	}

	public static async Task<CloudBlob> GetBlobReference(CloudBlobClient blobClient, Uri blobUri)
	{
		return (CloudBlob)(await blobClient.GetBlobReferenceFromServerAsync(blobUri));
	}

	public static async Task RequestInputBlob(CloudBlob cloudBlob, Stream inputStream)
	{
		await cloudBlob.DownloadToStreamAsync(inputStream);
	}

	public static async Task<CloudBlockBlob> GetImageBlobReference(CloudBlobClient blobClient, string blobName)
	{
		// Retrieve reference to a previously created container.
		CloudBlobContainer container = blobClient.GetContainerReference(ImageContainerName);

		// Create reference to a blob named "blobName".
		return container.GetBlockBlobReference(blobName);
	}
	
	public static async Task<CloudBlockBlob> GetThumbnailBlobReference(CloudBlobClient blobClient, string blobName)
	{
		// Retrieve reference to a previously created container.
		CloudBlobContainer container = blobClient.GetContainerReference(ThumbContainerName);

		// Create reference to a blob named "blobName".
		return container.GetBlockBlobReference(blobName);
	}

	public static string GetBlobName(CloudBlob cloudBlob)
	{
		return cloudBlob.Name;
	}

	public static string GetContainerName(CloudBlob cloudBlob)
	{
		return cloudBlob.Container.Name;
	}
}