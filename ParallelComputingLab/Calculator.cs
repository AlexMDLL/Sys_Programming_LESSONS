namespace ParallelComputingLab;

public static class Calculator
{
    /// <summary>
    /// Последовательная обработка массива.
    /// </summary>
    public static CalculationResult CalculateSequential(int[] numbers)
    {
        ArgumentNullException.ThrowIfNull(numbers);

        long sum = 0;
        int count = 0;
        int? max = null;

        foreach (int number in numbers)
        {
            if (number % 13 == 0)
            {
                sum += number;
                count++;

                if (max is null || number > max)
                {
                    max = number;
                }
            }
        }

        return new CalculationResult(sum, count, max);
    }

    /// <summary>
    /// Параллельная обработка массива.
    /// </summary>
    public static CalculationResult CalculateParallel(
        int[] numbers,
        int maxDegreeOfParallelism = -1)
    {
        ArgumentNullException.ThrowIfNull(numbers);

        if (maxDegreeOfParallelism != -1 &&
            maxDegreeOfParallelism < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxDegreeOfParallelism));
        }

        long sum = 0;
        int count = 0;
        int max = 0;

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism
        };

        Parallel.For(
            0,
            numbers.Length,
            options,
            () => (LocalSum: 0L, LocalCount: 0, LocalMax: 0),
            (i, state, local) =>
            {
                int number = numbers[i];

                if (number % 13 == 0)
                {
                    local.LocalSum += number;
                    local.LocalCount++;

                    if (number > local.LocalMax)
                    {
                        local.LocalMax = number;
                    }
                }

                return local;
            },
            local =>
            {
                Interlocked.Add(ref sum, local.LocalSum);
                Interlocked.Add(ref count, local.LocalCount);

                int current = max;

                while (local.LocalMax > current)
                {
                    int previous = Interlocked.CompareExchange(
                        ref max,
                        local.LocalMax,
                        current);

                    if (previous == current)
                    {
                        break;
                    }

                    current = previous;
                }
            });

        return new CalculationResult(sum, count, count == 0 ? null : max);
    }

    /// <summary>
    /// Исправленный вариант: локальное состояние Parallel.For
    /// и Interlocked.Add при объединении результатов.
    /// </summary>
    public static CalculationResult CalculateParallelWrong(int[] numbers)
    {
        ArgumentNullException.ThrowIfNull(numbers);

        long sum = 0;
        int count = 0;
        int max = 0;

        Parallel.For(
            0,
            numbers.Length,
            () => (LocalSum: 0L, LocalCount: 0, LocalMax: 0),
            (i, state, local) =>
            {
                int number = numbers[i];

                if (number % 13 == 0)
                {
                    local.LocalSum += number;
                    local.LocalCount++;

                    if (number > local.LocalMax)
                    {
                        local.LocalMax = number;
                    }
                }

                return local;
            },
            local =>
            {
                Interlocked.Add(ref sum, local.LocalSum);
                Interlocked.Add(ref count, local.LocalCount);

                int current = max;

                while (local.LocalMax > current)
                {
                    int previous = Interlocked.CompareExchange(
                        ref max,
                        local.LocalMax,
                        current);

                    if (previous == current)
                    {
                        break;
                    }

                    current = previous;
                }
            });

        return new CalculationResult(sum, count, count == 0 ? null : max);
    }
}
