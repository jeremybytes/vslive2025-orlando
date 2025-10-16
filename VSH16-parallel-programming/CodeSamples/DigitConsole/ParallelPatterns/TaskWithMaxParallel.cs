using Digits;

namespace DigitConsole;

public class TaskWithMaxParallel : DigitRunner
{
    private int maxParallel = Environment.ProcessorCount - 2;

    public override async Task RunClassifier(DigitImage[] validation, Classifier classifier, CancellationToken cancelToken)
    {
        using SemaphoreSlim semaphore = new(maxParallel);

        List<Task> allContinuations = new();

        foreach (var imageData in validation)
        {
            cancelToken.ThrowIfCancellationRequested();
            await semaphore.WaitAsync();

            var predictionTask = classifier.Predict(imageData);
            Task continuation = predictionTask.ContinueWith(t =>
            {
                try
                {
                    var prediction = t.Result;
                    lock (validation)
                    {
                        DisplayImages(prediction);
                    }

                    LogError(prediction);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            allContinuations.Add(continuation);
        }

        await Task.WhenAll(allContinuations);
    }

    public TaskWithMaxParallel(int count, int offset) :
        base(count, offset)
    {
    }

    public override string RunnerName => $"Task - Parallel with Max Parallel ({maxParallel})";

}
