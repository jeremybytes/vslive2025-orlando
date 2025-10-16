using Digits;

namespace DigitDesktop;

public class BasicTask : RecognizerControl
{
    protected override async Task Run(DigitImage[] validation, Classifier classifier)
    {
        List<Task> allContinuations = new();

        foreach (var imageData in validation)
        {
            var predictionTask = classifier.Predict(imageData);
            var continuation = predictionTask.ContinueWith(t =>
                {
                    var prediction = t.Result;
                    CreateUIElements(prediction);
                },
                TaskScheduler.FromCurrentSynchronizationContext()
            );
            allContinuations.Add(continuation);
        }
        await Task.WhenAll(allContinuations);
    }

    public BasicTask(string controlTitle, double displayMultiplier) :
        base($"{controlTitle} (Parallel Task)", displayMultiplier)
    { }
}
