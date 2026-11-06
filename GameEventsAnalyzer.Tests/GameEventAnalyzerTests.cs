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
}
