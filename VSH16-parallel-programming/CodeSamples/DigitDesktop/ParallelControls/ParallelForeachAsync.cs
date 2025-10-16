using Digits;

namespace DigitDesktop;

public class ParallelForeachAsync : RecognizerControl
{
    protected override async Task Run(DigitImage[] validation, Classifier classifier)
    {
        int maxParallel = Environment.ProcessorCount - 2;

        await Parallel.ForEachAsync(
            validation,
            new ParallelOptions()
            {
                MaxDegreeOfParallelism = maxParallel
            },
            async (imageData, _) =>
            {
                var prediction = await classifier.Predict(imageData);
                Dispatcher.Invoke(() => CreateUIElements(prediction));
            });
    }

    public ParallelForeachAsync(string controlTitle, double displayMultiplier) :
        base($"{controlTitle} (Parallel ForEachAsync)", displayMultiplier)
    { }
}
