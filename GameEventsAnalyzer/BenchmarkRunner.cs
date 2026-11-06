using System.Diagnostics;

namespace GameEventsAnalyzer;

// Сравнивает время LINQ и PLINQ на одних и тех же данных.
public static class BenchmarkRunner
{
    public static void Compare(
        List<GameEvent> events,
        int repetitions = 5)
    {
        ArgumentNullException.ThrowIfNull(events);

        ArgumentOutOfRangeException.ThrowIfLessThan(repetitions, 1);

        // Прогрев.
        GameEventAnalyzer.AnalyzeWithLinq(events);
        GameEventAnalyzer.AnalyzeWithPlinq(events);

        var linqTimes = new List<double>();
        var plinqTimes = new List<double>();

        for (int i = 0; i < repetitions; i++)
        {
            var (linqResult, linqElapsed) = Measure(
                () => GameEventAnalyzer.AnalyzeWithLinq(events));

            var (plinqResult, plinqElapsed) = Measure(
                () => GameEventAnalyzer.AnalyzeWithPlinq(events));

            if (!linqResult.SequenceEqual(plinqResult))
            {
                throw new InvalidOperationException(
                    "Результаты LINQ и PLINQ не совпадают.");
            }

            linqTimes.Add(linqElapsed);
            plinqTimes.Add(plinqElapsed);
        }

        Console.WriteLine($"LINQ:  {Median(linqTimes):F3} мс");
        Console.WriteLine($"PLINQ: {Median(plinqTimes):F3} мс");
    }

    public static void CompareOrdered(
        List<GameEvent> events,
        int repetitions = 5)
    {
        ArgumentNullException.ThrowIfNull(events);

        ArgumentOutOfRangeException.ThrowIfLessThan(repetitions, 1);

        // Прогрев.
        GameEventAnalyzer.GetKillsUnordered(events);
        GameEventAnalyzer.GetKillsOrdered(events);

        var unorderedTimes = new List<double>();
        var orderedTimes = new List<double>();

        for (int i = 0; i < repetitions; i++)
        {
            unorderedTimes.Add(MeasureKills(
                () => GameEventAnalyzer.GetKillsUnordered(events)));

            orderedTimes.Add(MeasureKills(
                () => GameEventAnalyzer.GetKillsOrdered(events)));
        }

        Console.WriteLine($"Kill без AsOrdered: {Median(unorderedTimes):F3} мс");
        Console.WriteLine($"Kill с AsOrdered:   {Median(orderedTimes):F3} мс");
    }

    private static double MeasureKills(Func<List<GameEvent>> action)
    {
        var stopwatch = Stopwatch.StartNew();
        action();
        stopwatch.Stop();
        return stopwatch.Elapsed.TotalMilliseconds;
    }

    public static (List<PlayerStatistics> Result, double ElapsedMilliseconds)
        Measure(Func<List<PlayerStatistics>> action)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = action();
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
