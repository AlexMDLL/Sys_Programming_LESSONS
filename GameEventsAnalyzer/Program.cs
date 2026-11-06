namespace GameEventsAnalyzer;

internal class Program
{
    static void Main(string[] args)
    {
        string mode = args.Length > 0
            ? args[0].ToLowerInvariant()
            : "all";

        Console.WriteLine("=== Game Events Analyzer ===");

        if (mode is "all" or "checks")
        {
            RunChecks();
        }

        if (mode is "all" or "order")
        {
            RunOrderDemo();
        }

        if (mode is "all" or "bench")
        {
            RunBenchmarks();
        }

        Console.WriteLine();
        Console.WriteLine("Работа завершена.");
    }

    private static void RunChecks()
    {
        List<GameEvent> events =
        [
            new(1, 1, "Kill", 100, DateTime.Now),
            new(2, 1, "Kill", 150, DateTime.Now),
            new(3, 2, "Kill", 200, DateTime.Now),
            new(4, 1, "Death", 0, DateTime.Now),
            new(5, 2, "Loot", 20, DateTime.Now)
        ];

        Console.WriteLine();
        Console.WriteLine("=== Контрольные сценарии ===");

        var linq = GameEventAnalyzer.AnalyzeWithLinq(events);
        var plinq = GameEventAnalyzer.AnalyzeWithPlinq(events);

        bool same = linq.SequenceEqual(plinq);
        Console.WriteLine($"Результаты LINQ и PLINQ совпадают: {same}");
        Console.WriteLine();

        foreach (var player in linq)
        {
            Console.WriteLine(
                $"Игрок {player.PlayerId}: " +
                $"убийств {player.KillCount}, " +
                $"очков {player.TotalPoints}");
        }

        var empty = GameEventAnalyzer.AnalyzeWithPlinq([]);
        Console.WriteLine();
        Console.WriteLine($"Пустая коллекция -> записей: {empty.Count}");
    }

    private static void RunOrderDemo()
    {
        List<GameEvent> events = GameEventGenerator.Generate(1_000_000);

        Console.WriteLine();
        Console.WriteLine("=== Лента событий ===");

        var unordered = GameEventAnalyzer.GetKillsUnordered(events);
        var ordered = GameEventAnalyzer.GetKillsOrdered(events);
        var sorted = GameEventAnalyzer.GetKillsSortedByTime(events);

        Console.WriteLine($"Kill без AsOrdered:    {unordered.Count}");
        Console.WriteLine($"Kill с AsOrdered:      {ordered.Count}");
        Console.WriteLine($"Kill после сортировки: {sorted.Count}");

        // TODO 3 (П2): проверьте, сохраняется ли исходный порядок
        // в варианте с AsOrdered.

        Console.WriteLine();
        Console.WriteLine("Первые 10 событий с сохранением исходного порядка:");

        foreach (var gameEvent in ordered.Take(10))
        {
            Console.WriteLine(
                $"ID: {gameEvent.Id}, " +
                $"Игрок: {gameEvent.PlayerId}, " +
                $"Время: {gameEvent.Timestamp:HH:mm:ss}");
        }
    }

    private static void RunBenchmarks()
    {
        // TODO 9 (П2): сравните время выполнения вариантов
        // с AsOrdered и без него.

        int[] sizes = [1_000, 10_000, 100_000, 1_000_000];

        foreach (int size in sizes)
        {
            List<GameEvent> events = GameEventGenerator.Generate(
                size,
                seed: 12345);

            Console.WriteLine();
            Console.WriteLine($"Событий: {size:N0}");

            BenchmarkRunner.Compare(events);
        }
    }
}
