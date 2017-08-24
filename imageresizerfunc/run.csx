#r "Microsoft.Azure.WebJobs.Extensions.EventGrid"
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Host.Bindings.Runtime;
using ImageResizer;
using ImageResizer.ExtensionMethods;

public static async Task Run(EventGridEvent e, Stream inputBlob, Binder binder, TraceWriter log)
{
    log.Info(e == null? "null event" : e.ToString());

    //Create a stream to write the image to    
    MemoryStream m = new MemoryStream();
    var instructions = new Instructions
    {
        Width = 150,
        Height = 150,
        Mode = FitMode.Crop,
        Scale = ScaleMode.Both
    };    
    ImageBuilder.Current.Build(new ImageJob(inputBlob, m, instructions));
    
    //reset the stream's position to the beginning
    m.Position = 0;

    //get the blobname from the event
    string blobname = e.Subject.Remove(0, e.Subject.LastIndexOf('/')+1);

    //setup the info for the new image
    var attributes = new Attribute[]
    {    
        new BlobAttribute(string.Format("thumbs/{0}", blobname), FileAccess.Write),
        new StorageAccountAttribute("zhstore2_STORAGE")
    };

    //write the resized image to a new blob
    using (var outputBlob = await binder.BindAsync<Stream>(attributes))
    {
        m.WriteTo(outputBlob);
    }
}

