using Digits;

namespace DigitDesktop;

public class TaskWithMaxParallel : RecognizerControl
{
    protected override async Task Run(DigitImage[] validation, Classifier classifier)
    {
        int maxParallel = Environment.ProcessorCount - 2;

        using SemaphoreSlim semaphore = new(maxParallel);
        List<Task> allContinuations = new();
        foreach (var imageData in validation)
        {
            await semaphore.WaitAsync();
            var predictionTask = classifier.Predict(imageData);
            var continuation = predictionTask.ContinueWith(t =>
                {
                    try
                    {
                        var prediction = t.Result;
                        CreateUIElements(prediction);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                },
                TaskScheduler.FromCurrentSynchronizationContext()
            );
            allContinuations.Add(continuation);
        }
        await Task.WhenAll(allContinuations);
    }

    public TaskWithMaxParallel(string controlTitle, double displayMultiplier) :
        base($"{controlTitle} (Parallel Constrained Task)", displayMultiplier)
    { }
}
