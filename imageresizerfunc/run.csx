using ImageResizer;
using ImageResizer.ExtensionMethods;

public static void Run(string myQueueItem, Stream inputBlob, Stream outputBlob, TraceWriter log)
{
    log.Info($"C# Queue trigger function processed: {myQueueItem}");
    
    var instructions = new Instructions
    {
        Width = 150,
        Height = 150,
        Mode = FitMode.Crop,
        Scale = ScaleMode.Both
    };
    ImageBuilder.Current.Build(new ImageJob(inputBlob, outputBlob, instructions));
}
