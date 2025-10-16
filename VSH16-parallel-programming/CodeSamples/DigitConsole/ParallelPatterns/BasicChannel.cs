using Digits;
using System.Threading.Channels;

namespace DigitConsole.ParallelPatterns;

public class BasicChannel : DigitRunner
{
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
            Classifier classifier, DigitImage imageData)
        {
            var prediction = await classifier.Predict(imageData);
            await writer.WriteAsync(prediction);
        }

        List<Task> allTasks = new();

        foreach (var imageData in validation)
        {
            cancelToken.ThrowIfCancellationRequested();
            Task fetchTask = FetchData(writer, classifier, imageData);
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

    public BasicChannel(int count, int offset) : base(count, offset)
    {
    }

    public override string RunnerName => "Basic Channel";
}
