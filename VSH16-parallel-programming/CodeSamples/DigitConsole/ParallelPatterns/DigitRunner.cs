using Digits;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace DigitConsole;

public abstract class DigitRunner
{
    private BlockingCollection<Prediction> errors = new();
    private int offset;
    private int count;

    public abstract string RunnerName { get; }
    public abstract Task RunClassifier(DigitImage[] validation, Classifier classifier, CancellationToken cancelToken);

    protected DigitRunner(int count, int offset)
    {
        this.count = count;
        this.offset = offset;
    }

    public async Task Run(CancellationToken cancelToken = new())
    {
        var (training, validation) = FileLoader.GetData("train.csv", offset, count);
        Console.WriteLine("Data Load Complete...");
        Console.Clear();

        Classifier classifier = new EuclideanClassifier(training);

        var timer = new Stopwatch();
        timer.Start();

        Console.Clear();
        await RunClassifier(validation, classifier, cancelToken);

        timer.Stop();
        var elapsed = timer.Elapsed;

        PrintSummary(classifier, offset, count, elapsed, errors.Count);

        // ERROR SECTION
        //Console.WriteLine("Press any key to show errors...");
        //Console.ReadLine();

        //foreach (var item in errors)
        //{
        //    DisplayImages(item, true);
        //}

        //PrintSummary(classifier, offset, count, elapsed, errors.Count);
    }

    internal void DisplayImages(Prediction prediction, bool scroll = false)
    {
        if (!scroll)
        {
            Console.SetCursorPosition(0, 0);
        }
        StringBuilder output = new();
        output.AppendLine(RunnerName);
        output.Append($"Actual: {prediction.Actual.Value} ");
        output.Append(' ', 46);
        output.AppendLine($" | Predicted: {prediction.Predicted.Value}");
        Display.GetImagesAsString(output, prediction.Actual.Image, prediction.Predicted.Image);
        output.Append('=', 115);
        Console.WriteLine(output);
    }

    internal void PrintSummary(Classifier classifier, int offset, int count, TimeSpan elapsed, int total_errors)
    {
        Console.WriteLine($"Using {classifier.Name} -- Offset: {offset}   Count: {count}");
        Console.WriteLine($"Total time: {elapsed}");
        Console.WriteLine($"Total errors: {total_errors}");
    }

    internal void LogError(Prediction prediction)
    {
        if (prediction.Actual.Value != prediction.Predicted.Value)
        {
            errors.Add(prediction);
        }
    }

}
