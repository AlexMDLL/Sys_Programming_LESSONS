using GameEventsAnalyzer;

namespace GameEventsAnalyzer.Tests;

public class GameEventAnalyzerTests
{
    private static readonly DateTime TestDate = new(2026, 1, 1);

    private static List<GameEvent> CreateEvents() =>
    [
        new(1, 1, "Kill", 100, TestDate),
        new(2, 1, "Kill", 150, TestDate),
        new(3, 2, "Kill", 200, TestDate),
        new(4, 1, "Death", 0, TestDate),
        new(5, 2, "Loot", 20, TestDate)
    ];

    [Fact]
    public void Linq_ReturnsStatisticsForKills()
    {
        var result = GameEventAnalyzer.AnalyzeWithLinq(CreateEvents());

        Assert.Equal(2, result.Count);
        Assert.Equal(new PlayerStatistics(1, 2, 250), result[0]);
        Assert.Equal(new PlayerStatistics(2, 1, 200), result[1]);
    }

    [Fact]
    public void Plinq_ReturnsSameStatisticsAsLinq()
    {
        var events = CreateEvents();

        var linq = GameEventAnalyzer.AnalyzeWithLinq(events);
        var plinq = GameEventAnalyzer.AnalyzeWithPlinq(events);

        Assert.Equal(linq, plinq);
    }

    [Fact]
    public void Statistics_AreSortedByPointsDescending()
    {
        var events = new List<GameEvent>
        {
            new(1, 3, "Kill", 100, TestDate),
            new(2, 1, "Kill", 300, TestDate),
            new(3, 2, "Kill", 250, TestDate)
        };

        var result = GameEventAnalyzer.AnalyzeWithPlinq(events);

        Assert.Equal(new[] { 1, 2, 3 }, result.Select(x => x.PlayerId));
    }

    [Fact]
    public void Statistics_WithEqualPoints_AreOrderedByPlayerId()
    {
        var events = new List<GameEvent>
        {
            new(1, 5, "Kill", 100, TestDate),
            new(2, 2, "Kill", 100, TestDate),
            new(3, 9, "Kill", 100, TestDate)
        };

        var linq = GameEventAnalyzer.AnalyzeWithLinq(events);
        var plinq = GameEventAnalyzer.AnalyzeWithPlinq(events);

        Assert.Equal(new[] { 2, 5, 9 }, linq.Select(x => x.PlayerId));
        Assert.Equal(linq, plinq);
    }

    [Fact]
    public void LinqAndPlinq_OnGeneratedData_AreSequenceEqual()
    {
        var events = GameEventGenerator.Generate(10_000, seed: 12345);

        var linq = GameEventAnalyzer.AnalyzeWithLinq(events);
        var plinq = GameEventAnalyzer.AnalyzeWithPlinq(events);

        Assert.True(linq.SequenceEqual(plinq));
    }

    [Fact]
    public void Analyze_EmptyCollection_ReturnsEmptyResult()
    {
        var result = GameEventAnalyzer.AnalyzeWithPlinq([]);

        Assert.Empty(result);
    }

    [Fact]
    public void Analyze_WithoutKills_ReturnsEmptyResult()
    {
        var events = new List<GameEvent>
        {
            new(1, 1, "Death", 0, TestDate),
            new(2, 2, "Loot", 20, TestDate)
        };

        var result = GameEventAnalyzer.AnalyzeWithPlinq(events);

        Assert.Empty(result);
    }

    [Fact]
    public void Analyze_AllKillsOfOnePlayer_ReturnsSingleRecord()
    {
        var events = new List<GameEvent>
        {
            new(1, 7, "Kill", 100, TestDate),
            new(2, 7, "Kill", 50, TestDate),
            new(3, 7, "Death", 0, TestDate)
        };

        var result = GameEventAnalyzer.AnalyzeWithPlinq(events);

        Assert.Single(result);
        Assert.Equal(new PlayerStatistics(7, 2, 150), result[0]);
    }

    // null отсекается сразу в начале метода, поэтому возникает
    // ArgumentNullException, а не NullReferenceException внутри LINQ/PLINQ.
    [Fact]
    public void Analyze_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => GameEventAnalyzer.AnalyzeWithLinq(null!));

        Assert.Throws<ArgumentNullException>(
            () => GameEventAnalyzer.AnalyzeWithPlinq(null!));
    }

    [Fact]
    public void GetKillsUnordered_ReturnsOnlyKills()
    {
        var result = GameEventAnalyzer.GetKillsUnordered(CreateEvents());

        Assert.Equal(3, result.Count);
        Assert.All(result, gameEvent => Assert.Equal("Kill", gameEvent.Type));
    }

    [Fact]
    public void GetKillsOrdered_PreservesSourceOrder()
    {
        var events = new List<GameEvent>
        {
            new(1, 1, "Kill", 100, TestDate),
            new(2, 1, "Death", 0, TestDate),
            new(3, 2, "Kill", 200, TestDate),
            new(4, 1, "Kill", 150, TestDate)
        };

        var result = GameEventAnalyzer.GetKillsOrdered(events);

        Assert.Equal(new[] { 1, 3, 4 }, result.Select(e => e.Id));
    }

    [Fact]
    public void GetKillsOrdered_OnGeneratedData_ReturnsAscendingIds()
    {
        var events = GameEventGenerator.Generate(10_000, seed: 12345);

        var result = GameEventAnalyzer.GetKillsOrdered(events);

        Assert.True(result
            .Select(e => e.Id)
            .SequenceEqual(result.Select(e => e.Id).OrderBy(id => id)));
    }

    [Fact]
    public void GetKillsSortedByTime_SortsEvents()
    {
        var events = new List<GameEvent>
        {
            new(1, 1, "Kill", 100, TestDate.AddMinutes(5)),
            new(2, 1, "Kill", 150, TestDate.AddMinutes(1)),
            new(3, 2, "Kill", 200, TestDate.AddMinutes(3))
        };

        var result = GameEventAnalyzer.GetKillsSortedByTime(events);

        Assert.Equal(new[] { 2, 3, 1 }, result.Select(e => e.Id));
    }

    [Fact]
    public void GetKillsSortedByTime_WithEqualTimestamps_SortsById()
    {
        var events = new List<GameEvent>
        {
            new(3, 1, "Kill", 100, TestDate),
            new(1, 1, "Kill", 100, TestDate),
            new(2, 1, "Kill", 100, TestDate)
        };

        var result = GameEventAnalyzer.GetKillsSortedByTime(events);

        Assert.Equal(new[] { 1, 2, 3 }, result.Select(e => e.Id));
    }
}
