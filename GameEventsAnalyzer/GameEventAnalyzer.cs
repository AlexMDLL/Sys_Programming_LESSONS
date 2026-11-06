namespace GameEventsAnalyzer;

public static partial class GameEventAnalyzer
{
    public static List<PlayerStatistics> AnalyzeWithLinq(
        IEnumerable<GameEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        return events
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

    // TODO 4 (П2): отсортируйте события Kill по Timestamp.
    // TODO 5 (П2): добавьте ThenBy по Id.
    public static List<GameEvent> GetKillsSortedByTime(
        IEnumerable<GameEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        throw new NotImplementedException("TODO 4 (П2): GetKillsSortedByTime");
    }
}
