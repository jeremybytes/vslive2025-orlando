using Digits;

namespace DigitConsole;

public class ParallelForeachAsync : DigitRunner
{
    int maxParallel = Environment.ProcessorCount - 2;

    public override async Task RunClassifier(DigitImage[] validation, Classifier classifier, CancellationToken cancelToken)
    {
        await Parallel.ForEachAsync(
            validation,
            new ParallelOptions() 
            {
                CancellationToken = cancelToken,
                MaxDegreeOfParallelism = maxParallel,
            },
            async (imageData, _) =>
            {
                var prediction = await classifier.Predict(imageData);
                lock (validation)
                {
                    DisplayImages(prediction);
                }

                LogError(prediction);
            });
    }

    public ParallelForeachAsync(int count, int offset) :
        base(count, offset)
    {
    }

    public override string RunnerName => $"Parallel.ForEachAsync - Max Parallel ({maxParallel})";
}
