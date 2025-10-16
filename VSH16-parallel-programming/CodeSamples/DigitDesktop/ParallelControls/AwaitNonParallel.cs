using Digits;

namespace DigitDesktop;

public class AwaitNonParallel : RecognizerControl
{
    protected override async Task Run(DigitImage[] validation, Classifier classifier)
    {
        foreach (var imageData in validation)
        {
            var prediction = await classifier.Predict(imageData);
            CreateUIElements(prediction);
        }
    }

    public AwaitNonParallel(string controlTitle, double displayMultiplier) :
        base($"{controlTitle} (Non-Parallel)", displayMultiplier)
    { }
}
