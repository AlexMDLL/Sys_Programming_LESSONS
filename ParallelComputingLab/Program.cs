namespace ParallelComputingLab;

internal class Program
{
    static void Main()
    {
        Console.WriteLine("=== Parallel Computing Lab ===");

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

        // П2. TODO 7: повторите эксперимент после перезапуска программы
        // и сравните результаты.
        // П2. TODO 10: сделайте вывод, всегда ли параллельная обработка
        // быстрее последовательной.

        Console.WriteLine("Работа завершена.");
    }
}
