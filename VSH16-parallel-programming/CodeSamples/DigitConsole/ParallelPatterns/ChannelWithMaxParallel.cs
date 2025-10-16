using Digits;
using System.Threading.Channels;

namespace DigitConsole;

public class ChannelWithMaxParallel : DigitRunner
{
    private int maxParallel = Environment.ProcessorCount - 2;

    public override async Task RunClassifier(DigitImage[] validation, Classifier classifier, CancellationToken cancelToken)
    {
        this.cancelToken = cancelToken;

        var channel = Channel.CreateUnbounded<Prediction>();

        var listener = Listen(channel.Reader);
        var producer = Produce(channel.Writer, validation, classifier);

        await producer;
        await listener;
    }

    private async Task Produce(ChannelWriter<Prediction> writer, DigitImage[] validation, Classifier classifier)
    {
        static async Task FetchData(ChannelWriter<Prediction> writer, 
            Classifier classifier, DigitImage imageData, 
            SemaphoreSlim semaphore)
        {
            try
            {
                var prediction = await classifier.Predict(imageData);
                await writer.WriteAsync(prediction);
            }
            finally
            {
                semaphore.Release();
            }
        }

        using SemaphoreSlim semaphore = new(maxParallel);

        List<Task> allTasks = new();

        foreach (var imageData in validation)
        {
            cancelToken.ThrowIfCancellationRequested();
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
            DisplayImages(prediction);
            LogError(prediction);
        }
    }

    private CancellationToken cancelToken = CancellationToken.None;

    public ChannelWithMaxParallel(int count, int offset) :
        base(count, offset)
    {
    }

    public override string RunnerName => $"Channel with Max Parallel ({maxParallel})";
}
