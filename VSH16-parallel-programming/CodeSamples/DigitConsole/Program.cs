using DigitConsole.ParallelPatterns;

namespace DigitConsole;

internal class Program
{
    private static int count = 1000;
    private static int offset = 1000;
    private static CancellationTokenSource tokenSource = new();

    static void Main(string[] args)
    {
        Console.Clear();
        Console.WriteLine("Please make a selection:");
        Console.WriteLine("1) Await (non-parallel)");
        Console.WriteLine("2) Basic Task (parallel)");
        Console.WriteLine("3) Task with max parallel (parallel)");
        Console.WriteLine("4) Parallel.ForEachAsync (parallel)");
        Console.WriteLine("5) Basic Channel (parallel)");
        Console.WriteLine("6) Channel with max parallel (parallel)");
        Console.WriteLine("(any other key to exit)");

        switch (Console.ReadKey().Key)
        {
            case ConsoleKey.D1:
            case ConsoleKey.NumPad1:
                Console.WriteLine();
                _ = CallRunner(new AwaitNonParallel(count, offset));
                HandleExit();
                break;
            case ConsoleKey.D2:
            case ConsoleKey.NumPad2:
                Console.WriteLine();
                _ = CallRunner(new BasicTask(count, offset));
                HandleExit();
                break;
            case ConsoleKey.D3:
            case ConsoleKey.NumPad3:
                Console.WriteLine();
                _ = CallRunner(new TaskWithMaxParallel(count, offset));
                HandleExit();
                break;
            case ConsoleKey.D4:
            case ConsoleKey.NumPad4:
                Console.WriteLine();
                _ = CallRunner(new ParallelForeachAsync(count, offset));
                HandleExit();
                break;
            case ConsoleKey.D5:
            case ConsoleKey.NumPad5:
                Console.WriteLine();
                _ = CallRunner(new BasicChannel(count, offset));
                HandleExit();
                break;
            case ConsoleKey.D6:
            case ConsoleKey.NumPad6:
                Console.WriteLine();
                _ = CallRunner(new ChannelWithMaxParallel(count, offset));
                HandleExit();
                break;
            default:
                Console.WriteLine();
                Environment.Exit(0);
                break;
        }
    }

    private static async Task CallRunner(DigitRunner runner)
    {
        Console.WriteLine("One Moment Please ('x' to Cancel, 'q' to Quit)");

        Task continuation = runner.Run(tokenSource.Token);

        await continuation.ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Console.WriteLine("\nThere was a problem retrieving data");
                foreach (var ex in task.Exception!.Flatten().InnerExceptions)
                    Console.WriteLine($"\nERROR\n{ex.GetType()}\n{ex.Message}");
                Environment.Exit(1);
            }
            if (task.IsCanceled)
            {
                Console.WriteLine("\nThe operation was canceled");
                Environment.Exit(0);
            }
            if (task.IsCompletedSuccessfully)
            {
                Console.WriteLine("\nThe operation completed successfully");
                Environment.Exit(0);
            }
        });
    }

    private static void HandleExit()
    {
        bool Cancel() { tokenSource.Cancel(); return true; };
        bool Exit() { Environment.Exit(0); return true; };
        bool Ignore() => true;

        while (true)
        {
            _ = Console.ReadKey().Key switch
            {
                ConsoleKey.X => Cancel(),
                ConsoleKey.Q => Exit(),
                _ => Ignore(),
            };
        }
    }
}
