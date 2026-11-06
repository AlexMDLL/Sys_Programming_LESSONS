using System.Diagnostics;

namespace ParallelComputingLab;

// Класс проводит измерения и выводит медианное время.
public static class BenchmarkRunner
{
    // П2. TODO 3: измените количество измерений с 5 на 10.
    private const int MeasurementCount = 10;

    public static void Run(int[] numbers)
    {
        ArgumentNullException.ThrowIfNull(numbers);

        Console.WriteLine();
        Console.WriteLine($"Размер массива: {numbers.Length:N0}");

        // Прогрев обеих реализаций.
        Calculator.CalculateSequential(numbers);
        Calculator.CalculateParallel(numbers);

        var sequentialTimes = new List<double>();
        var parallelTimes = new List<double>();

        CalculationResult? expectedResult = null;

        for (int i = 0; i < MeasurementCount; i++)
        {
            var sequential = Measure(
                () => Calculator.CalculateSequential(numbers));

            var (Result, ElapsedMilliseconds) = Measure(
                () => Calculator.CalculateParallel(numbers));

            expectedResult ??= sequential.Result;

            if (sequential.Result != Result)
            {
                throw new InvalidOperationException(
                    "Последовательный и параллельный результаты не совпадают.");
            }

            sequentialTimes.Add(sequential.ElapsedMilliseconds);
            parallelTimes.Add(ElapsedMilliseconds);
        }

        Console.WriteLine(
            $"Сумма:           {expectedResult!.Sum:N0}");
        Console.WriteLine(
            $"Количество:      {expectedResult.Count:N0}");
        Console.WriteLine(
            $"Последовательно: {Median(sequentialTimes):F3} мс");
        Console.WriteLine(
            $"Параллельно:     {Median(parallelTimes):F3} мс");
    }

    public static (double Sequential, List<(int Degree, double Median)> Variants)
        CompareParallelism(int[] numbers)
    {
        ArgumentNullException.ThrowIfNull(numbers);

        // П2. TODO 2: добавьте степень параллелизма 3.
        // -1 означает "значение по умолчанию, без явного ограничения".
        int[] degrees = [1, 2, 3, 4, -1];

        Console.WriteLine();
        Console.WriteLine($"Размер массива: {numbers.Length:N0}");
        Console.WriteLine("Степень параллелизма | Время, мс | Ускорение");
        Console.WriteLine("--------------------------------------------");

        // Прогрев.
        Calculator.CalculateSequential(numbers);

        foreach (int degree in degrees)
        {
            Calculator.CalculateParallel(numbers, degree);
        }

        var sequentialTimes = new List<double>();

        for (int i = 0; i < MeasurementCount; i++)
        {
            var (_, ElapsedMilliseconds) = Measure(
                () => Calculator.CalculateSequential(numbers));

            sequentialTimes.Add(ElapsedMilliseconds);
        }

        double sequentialMedian = Median(sequentialTimes);
        var variants = new List<(int Degree, double Median)>();

        foreach (int degree in degrees)
        {
            var times = new List<double>();

            for (int i = 0; i < MeasurementCount; i++)
            {
                var (_, ElapsedMilliseconds) = Measure(
                    () => Calculator.CalculateParallel(numbers, degree));

                times.Add(ElapsedMilliseconds);
            }

            double median = Median(times);
            variants.Add((degree, median));

            // П2. TODO 4: добавьте в таблицу ускорение:
            // время последовательной обработки / время параллельной.
            // П2. TODO 9: добавьте проверку совпадения результата
            // для каждого измеряемого варианта.
            Console.WriteLine(
                $"{degree,20} | {median,10:F3} | {sequentialMedian / median,8:F2}x");
        }

        return (sequentialMedian, variants);
    }

    public static void PrintSummary(
        List<(int Size, double Sequential, List<(int Degree, double Median)> Variants)> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        Console.WriteLine();
        Console.WriteLine("=== Итоговая таблица (медиана, мс) ===");
        Console.WriteLine("Размер     | Последовательно | Deg 1 | Deg 2 | Deg 3 | Deg 4 | Default");
        Console.WriteLine("---------------------------------------------------------------------");

        foreach (var row in rows)
        {
            Console.Write($"{row.Size,10:N0} | {row.Sequential,15:F3}");

            foreach (var variant in row.Variants)
            {
                Console.Write($" | {variant.Median,5:F3}");
            }

            Console.WriteLine();
        }

        int? firstFaster = null;

        foreach (var row in rows)
        {
            double best = row.Variants.Min(v => v.Median);

            if (best < row.Sequential)
            {
                firstFaster ??= row.Size;
            }
        }

        Console.WriteLine();
        Console.WriteLine(
            firstFaster is null
                ? "Параллельная обработка не стала быстрее ни на одном размере."
                : $"Параллельная обработка быстрее последовательной начиная с размера {firstFaster:N0}.");
    }

    public static (CalculationResult Result, double ElapsedMilliseconds)
        Measure(Func<CalculationResult> action)
    {
        var stopwatch = Stopwatch.StartNew();
        CalculationResult result = action();
        stopwatch.Stop();
        return (result, stopwatch.Elapsed.TotalMilliseconds);
    }

    private static double Median(List<double> values)
    {
        double[] sorted = [.. values.OrderBy(x => x)];
        int middle = sorted.Length / 2;

        if (sorted.Length % 2 == 1)
        {
            return sorted[middle];
        }

        return (sorted[middle - 1] + sorted[middle]) / 2;
    }
}
