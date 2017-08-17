#r "Microsoft.Azure.WebJobs.Extensions.EventGrid"
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using ImageResizer;
using ImageResizer.ExtensionMethods;

public static void Run(EventGridEvent e, Stream inputBlob, Stream outputBlob, TraceWriter log)
{
    log.Info(e == null? "null event" : e.ToString());

    var instructions = new Instructions
    {
        Width = 150,
        Height = 150,
        Mode = FitMode.Crop,
        Scale = ScaleMode.Both
    };
    ImageBuilder.Current.Build(new ImageJob(inputBlob, outputBlob, instructions));
}

