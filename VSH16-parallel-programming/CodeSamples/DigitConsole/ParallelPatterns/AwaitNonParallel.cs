using Digits;

namespace DigitConsole;

public class AwaitNonParallel : DigitRunner
{
    public override async Task RunClassifier(DigitImage[] validation, Classifier classifier, CancellationToken cancelToken)
    {
        foreach (var imageData in validation)
        {
            cancelToken.ThrowIfCancellationRequested();

            var prediction = await classifier.Predict(imageData);
            DisplayImages(prediction);

            LogError(prediction);
        }
    }

    public AwaitNonParallel(int count, int offset) :
        base(count, offset)
    { }

    public override string RunnerName => "Await - Non Parallel";
}
