using AsyncMonitor;
using System.Diagnostics;
using System.Xml.Linq;
var passed = 0;
var failed = new List<string>();
var count = 0;
void Check( bool condition, string name)
{
    var previous = Console.ForegroundColor;
    try
    {
        Console.ForegroundColor = condition ? ConsoleColor.Green : ConsoleColor.Red;    // Я просто хочу красиво, простите
        Console.WriteLine($"{(condition ? "PASS" : "FAIL")} — {name}");
    }
    finally { Console.ForegroundColor = previous; }

    if (condition) { passed++; count++; }
    else { failed.Add(name); count++; }
}
var s = new CheckService();
var one = await s.CheckServiceAsync("Тест", TimeSpan.FromMilliseconds(205));
Check(
    one.Name == "Тест" && one.Success && one.Duration >= TimeSpan.FromMilliseconds(150),
    "одиночная проверка возвращает результат"
);
var seq = await s.RunSequentialAsync(
    [
        ("A", TimeSpan.FromMilliseconds(60)),
        ("B", TimeSpan.FromMilliseconds(60)),
        ("C", TimeSpan.FromMilliseconds(60))
    ]
);
Check(
    seq.Count == 3 && seq.Select(x => x.Name).SequenceEqual(new[] { "A", "B", "C" }),
    "последовательный запуск"
);
var all = await s.RunParallelAsync(
    [
        ("A", TimeSpan.FromMilliseconds(60)),
        ("B", TimeSpan.FromMilliseconds(60)),
        ("C", TimeSpan.FromMilliseconds(60))
    ]
);
Check(all.Count == 3 && all.All(x => x.Success), "параллельный запуск WhenAll");
// TODO: Добавить тесты на пустое имя, отрицательную задержку и CancellationToken.
var NamePUSTO = false;
try { await s.CheckServiceAsync("  ", TimeSpan.FromMilliseconds(10)); }
catch (ArgumentException) { NamePUSTO = true; }
Check(NamePUSTO, "пустое имя отклонено");

var Zaderjka200 = false;
try { await s.CheckServiceAsync("Тест", TimeSpan.FromMilliseconds(-5)); }
catch (ArgumentOutOfRangeException) { Zaderjka200 = true;}
Check(Zaderjka200, "отрицательная задержка отклонена");

var WorkCensel = false;
try
{
    using var cts = new CancellationTokenSource(50);
    await s.CheckServiceAsync("Отмена", TimeSpan.FromSeconds(5), cts.Token);
}
catch (OperationCanceledException) { WorkCensel = true; }
Check(WorkCensel, "CancellationToken отменяет ожидание");

var He_He_He = Stopwatch.StartNew();
await s.RunSequentialAsync(new[]
{
    ("A", TimeSpan.FromMilliseconds(60)),
    ("B", TimeSpan.FromMilliseconds(60)),
    ("C", TimeSpan.FromMilliseconds(60))
});
He_He_He.Stop();
var Merry_Cristmasx = Stopwatch.StartNew();
await s.RunParallelAsync(new[]
{
    ("A", TimeSpan.FromMilliseconds(60)),
    ("B", TimeSpan.FromMilliseconds(60)),
    ("C", TimeSpan.FromMilliseconds(60))
});
Merry_Cristmasx.Stop();
//Check( Merry_Cristmasx.Elapsed < He_He_He.Elapsed, $"параллельно быстрее последовательного ({Merry_Cristmasx.ElapsedMilliseconds} мс против {He_He_He.ElapsedMilliseconds} мс)" );
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine($"Пройденно проверок: {passed} из {count}.");
Console.ResetColor();
Console.ForegroundColor = ConsoleColor.Red;
Console.WriteLine($"Проваленно проверок: {failed.Count} из {count}.");
Console.ResetColor();
