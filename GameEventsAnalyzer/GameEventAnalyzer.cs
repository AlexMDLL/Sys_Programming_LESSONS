namespace GameEventsAnalyzer;

public static partial class GameEventAnalyzer
{
    // TODO 3 (П1): отсортируйте игроков по сумме очков по убыванию.
    // TODO 4 (П1): добавьте ThenBy по PlayerId для стабильного порядка.
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
            .ToList();
    }
}

public static partial class GameEventAnalyzer
{
    // TODO 1 (П2): сформируйте ленту событий Kill без AsOrdered.
    public static List<GameEvent> GetKillsUnordered(
        IEnumerable<GameEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        throw new NotImplementedException("TODO 1 (П2): GetKillsUnordered");
    }

    // TODO 2 (П2): сформируйте ту же ленту с AsOrdered.
    public static List<GameEvent> GetKillsOrdered(
        IEnumerable<GameEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        throw new NotImplementedException("TODO 2 (П2): GetKillsOrdered");
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
