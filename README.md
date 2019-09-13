---
page_type: sample
languages:
- csharp
products:
- azure
description: "This sample demonstrates how to respond to an EventGridEvent published by a storage account to resize an image and upload a thumbnail as described in the article Automate resizing uploaded images using Event Grid."
urlFragment: function-image-upload-resize
---

# Image Upload Resize 

This sample demonstrates how to respond to an `EventGridEvent` published by a storage account to resize  an image and upload a thumbnail as described in the article [Automate resizing uploaded images using Event Grid](https://docs.microsoft.com/azure/event-grid/resize-images-on-storage-blob-upload-event?toc=%2Fazure%2Fazure-functions%2Ftoc.json&tabs=net).

## Local Setup

Before running this sample locally, you need to add your connection string to the `AzureWebJobsStorage` value in a file named `local.settings.json` file. This file is excluded from the git repository, so an example file named `local.settings.example.json` is provided.

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "<STORAGE_ACCOUNT_CONNECTION_STRING>",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "THUMBNAIL_CONTAINER_NAME": "thumbnails",
    "THUMBNAIL_WIDTH":  "100",
    "datatype": "binary"
  }
}
```

To use this file, do the following steps:

1. Replace `<STORAGE_ACCOUNT_CONNECTION_STRING>` with your storage account connection string
2. Rename the file from `local.settings.example.json` to `local.settings.json` 

## Version Support

The `master` branch of this repository contains the Functions version 2.x implementation, while the `v1` branch has the Functions 1.x implementation.

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
