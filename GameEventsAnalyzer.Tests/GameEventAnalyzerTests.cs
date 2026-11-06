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
}
