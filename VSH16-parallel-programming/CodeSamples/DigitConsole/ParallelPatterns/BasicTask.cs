using Digits;

namespace DigitConsole;

public class BasicTask : DigitRunner
{
    public override async Task RunClassifier(DigitImage[] validation, Classifier classifier, CancellationToken cancelToken)
    {
        List<Task> allContinuations = new();

        foreach (var imageData in validation)
        {
            cancelToken.ThrowIfCancellationRequested();

            var predictionTask = classifier.Predict(imageData);
            Task continuation = predictionTask.ContinueWith(t =>
            {
                var prediction = t.Result;
                lock (validation)
                {
                    DisplayImages(prediction);
                }

                LogError(prediction);
            });

            allContinuations.Add(continuation);
        }

        await Task.WhenAll(allContinuations);
    }

    public BasicTask(int count, int offset) :
        base(count, offset)
    { }

    public override string RunnerName => "Basic Task - Parallel";

}
