namespace GameEventsAnalyzer;

public static partial class GameEventAnalyzer
{
    public static List<PlayerStatistics> AnalyzeWithLinq(
        IEnumerable<GameEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        return [.. events
            .Where(e => e.Type == "Kill")
            .GroupBy(e => e.PlayerId)
            .Select(group => new PlayerStatistics(
                PlayerId: group.Key,
                KillCount: group.Count(),
                TotalPoints: group.Sum(e => (long)e.Points)))
            .OrderByDescending(x => x.TotalPoints)
            .ThenBy(x => x.PlayerId)];
    }

    // TODO 2 (П1): реализуйте тот же анализ через PLINQ (AsParallel).
    public static List<PlayerStatistics> AnalyzeWithPlinq(
        IEnumerable<GameEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        return events
            .AsParallel()
            .Where(e => e.Type == "Kill")
            .GroupBy(e => e.PlayerId)
            .Select(group => new PlayerStatistics(
                PlayerId: group.Key,
                KillCount: group.Count(),
                TotalPoints: group.Sum(e => (long)e.Points)))
            .OrderByDescending(x => x.TotalPoints)
            .ThenBy(x => x.PlayerId)
            .ToList();
    }
}

public static partial class GameEventAnalyzer
{
    public static List<GameEvent> GetKillsUnordered(
        IEnumerable<GameEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        return events
            .AsParallel()
            .Where(e => e.Type == "Kill")
            .ToList();
    }

    public static List<GameEvent> GetKillsOrdered(
        IEnumerable<GameEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        return events
            .AsParallel()
            .AsOrdered()
            .Where(e => e.Type == "Kill")
            .ToList();
    }

    public static List<GameEvent> GetKillsSortedByTime(
        IEnumerable<GameEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        // AsOrdered() здесь не нужен: порядок задаёт явная сортировка.
        // Проверено: добавление AsOrdered() к запросу с OrderBy не меняет
        // итоговый порядок (см. тест Sorted_WithAndWithoutAsOrdered_ReturnSameOrder).
        return events
            .AsParallel()
            .Where(e => e.Type == "Kill")
            .OrderBy(e => e.Timestamp)
            // ThenBy(Id) делает порядок детерминированным: у событий бывают
            // одинаковые Timestamp (генератор выдаёт такие пары), и без
            // вторичного ключа их взаимный порядок в результате не определён.
            .ThenBy(e => e.Id)
            .ToList();
    }
}
