using Digits;
using System.Threading.Channels;
using System.Windows.Controls;

namespace DigitDesktop;

public class BasicChannel : RecognizerControl
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
            Classifier classifier, DigitImage imageData)
        {
            var prediction = await classifier.Predict(new(imageData.Value, imageData.Image));
            await writer.WriteAsync(prediction);
        }

        List<Task> allTasks = new();

        foreach (var imageData in validation)
        {
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
            CreateUIElements(prediction);
        }
    }

    public BasicChannel(string controlTitle, double displayMultiplier) :
    base($"{controlTitle} (Parallel Channel)", displayMultiplier)
    { }
}
