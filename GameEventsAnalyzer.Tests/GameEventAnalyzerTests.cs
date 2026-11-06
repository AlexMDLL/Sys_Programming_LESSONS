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
}
