namespace ParallelComputingLab;

internal class Program
{
    static void Main(string[] args)
    {
        string mode = args.Length > 0
            ? args[0].ToLowerInvariant()
            : "all";

        Console.WriteLine("=== Parallel Computing Lab ===");

        if (mode is "all" or "checks")
        {
            DemonstrateControlScenarios();
        }

        if (mode is "all" or "bench")
        {
            RunBenchmarks();
        }

        Console.WriteLine();
        Console.WriteLine("Работа завершена.");
    }

    private static void RunBenchmarks()
    {
        // П2. TODO 1: добавьте размер массива 10_000_000.
        int[] sizes =
        [
            10_000,
            100_000,
            1_000_000,
            5_000_000
        ];

        foreach (int size in sizes)
        {
            int[] numbers = DataGenerator.Generate(
                size,
                seed: 12345);

            BenchmarkRunner.Run(numbers);
            BenchmarkRunner.CompareParallelism(numbers);

            Console.WriteLine();
        }
    }

    private static void DemonstrateControlScenarios()
    {
        Console.WriteLine();
        Console.WriteLine("=== Контрольные сценарии ===");

        ShowScenario([13, 26, 40, 39]);
        ShowScenario([1, 2, 3, 4]);
        ShowScenario([13, 13, 13]);
        ShowScenario([26]);
        ShowScenario([]);
        ShowNullCheck();
        ShowWrongSum();
    }

    private static void ShowWrongSum()
    {
        int[] numbers = DataGenerator.Generate(
            5_000_000,
            seed: 12345);

        var correct = Calculator.CalculateSequential(numbers);
        var wrong = Calculator.CalculateParallelWrong(numbers);

        Console.WriteLine();
        Console.WriteLine("=== Lost update на 5 000 000 элементов ===");
        Console.WriteLine(
            $"Правильно: сумма={correct.Sum}, количество={correct.Count}");
        Console.WriteLine(
            $"С гонкой:  сумма={wrong.Sum}, количество={wrong.Count}");
        Console.WriteLine(
            $"Потеряно обновлений: {correct.Count - wrong.Count}");
    }

    private static void ShowNullCheck()
    {
        Console.WriteLine();
        Console.WriteLine("=== Проверка null ===");

        try
        {
            Calculator.CalculateParallel(null!);
            Console.WriteLine("Ожидалось ArgumentNullException, но исключения не было.");
        }
        catch (ArgumentNullException ex)
        {
            Console.WriteLine(
                $"Исключение: {ex.GetType().Name}, параметр: {ex.ParamName}");
        }
    }

    private static void ShowScenario(int[] numbers)
    {
        var sequential = Calculator.CalculateSequential(numbers);
        var parallel = Calculator.CalculateParallel(numbers);

        Console.WriteLine(
            $"[{Format(numbers)}] -> " +
            $"сумма={sequential.Sum}, " +
            $"количество={sequential.Count}, " +
            $"максимум={sequential.Max?.ToString() ?? "—"}, " +
            $"совпадает={sequential == parallel}");
    }

    private static string Format(int[] numbers) =>
        numbers.Length == 0
            ? "пусто"
            : string.Join(", ", numbers);
}
