using Digits;
using System.Threading.Channels;
using System.Windows.Controls;

namespace DigitDesktop;

public class ChannelWithMaxParallel : RecognizerControl
{
    protected override async Task Run(DigitImage[] validation, Classifier classifier)
    {
        var channel = Channel.CreateUnbounded<Prediction>();

        var listener = Listen(channel.Reader);
        var producer = Produce(channel.Writer, validation, classifier);

        await producer;
        await listener;
    }

    private async Task Produce(ChannelWriter<Prediction> writer, DigitImage[] validation,
        Classifier classifier)
    {
        static async Task FetchData(ChannelWriter<Prediction> writer, 
            Classifier classifier, DigitImage imageData, 
            SemaphoreSlim semaphore)
        {
            try
            {
                var prediction = await classifier.Predict(new(imageData.Value, imageData.Image));
                await writer.WriteAsync(prediction);
            }
            finally
            {
                semaphore.Release();
            }
        }

        int maxParallel = Environment.ProcessorCount - 2;
        using SemaphoreSlim semaphore = new(maxParallel);

        List<Task> allTasks = new();

        foreach (var imageData in validation)
        {
            await semaphore.WaitAsync();
            Task fetchTask = FetchData(writer, classifier, imageData, semaphore);
            allTasks.Add(fetchTask);
        }

        await Task.WhenAll(allTasks);
        writer.Complete();
    }

    private async Task Listen(ChannelReader<Prediction> reader)
    {
        await foreach (var prediction in reader.ReadAllAsync())
        {
            CreateUIElements(prediction);
        }
    }

    public ChannelWithMaxParallel(string controlTitle, double displayMultiplier) :
    base($"{controlTitle} (Parallel Channel)", displayMultiplier)
    { }
}

