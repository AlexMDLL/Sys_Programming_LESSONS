namespace ParallelComputingLab;

public static class Calculator
{
    /// <summary>
    /// Последовательная обработка массива.
    /// </summary>
    public static CalculationResult CalculateSequential(int[] numbers)
    {
        ArgumentNullException.ThrowIfNull(numbers);

        // TODO (П1): реализуйте последовательную обработку через foreach:
        // сумма и количество чисел, кратных 7.
        // П1. TODO 1: после реализации замените условие отбора на кратность 13.
        // П1. TODO 2: добавьте вычисление максимального подходящего элемента.
        throw new NotImplementedException(
            "TODO (П1): реализуйте CalculateSequential.");
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

        // TODO (П1): реализуйте Parallel.For с локальным состоянием
        // (LocalSum, LocalCount) и Interlocked.Add после завершения
        // каждой локальной части. Не меняйте сигнатуру метода.
        // П1. TODO 4: проверьте пустой массив.
        // П1. TODO 5: проверьте null и объясните, какое исключение
        // должно возникнуть.
        throw new NotImplementedException(
            "TODO (П1): реализуйте CalculateParallel.");
    }

    /// <summary>
    /// Заведомо неправильный вариант с гонкой данных.
    /// </summary>
    /// <remarks>
    /// П1. TODO 6: запустите метод на массиве из нескольких миллионов
    /// элементов и сравните его результат с правильным.
    /// П1. TODO 7: исправьте метод, не меняя его имя, через локальное
    /// состояние Parallel.For.
    /// </remarks>
    public static CalculationResult CalculateParallelWrong(int[] numbers)
    {
        ArgumentNullException.ThrowIfNull(numbers);

        long sum = 0;
        int count = 0;

        Parallel.For(0, numbers.Length, i =>
        {
            if (numbers[i] % 7 == 0)
            {
                // ОШИБКА: общие переменные изменяются несколькими
                // потоками без синхронизации. Ошибка проявляется
                // не обязательно при каждом запуске.
                sum += numbers[i];
                count++;
            }
        });

        return new CalculationResult(sum, count);
    }
}
