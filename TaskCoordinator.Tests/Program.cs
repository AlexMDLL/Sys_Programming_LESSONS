using TaskCoordinator;
var passed = 0;
var failed = new List<string>();
var count = 0;
void Check(bool condition, string name)
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

var s = new DiagnosticService();
var one = await s.RunAsync("Сеть", TimeSpan.FromMilliseconds(20));
Check(one.Status == DiagnosticStatus.Completed, "одна диагностика завершается");
var all = await s.RunAllAsync();
Check(
    all.Count == 3 && all.All(x => x.Status == DiagnosticStatus.Completed),
    "WhenAll ждёт три диагностики"//нет не ждет, это я его жду 😒
);
var first = await s.Kto_Perviy();
Check(first == "Быстрая", "WhenAny возвращает первую завершившуюся");
// TODO: Добавить тесты кооперативной отмены, тайм-аута, неверного имени и отрицательного времени.

var cancelled = false;
using (var cts = new CancellationTokenSource(150))
{
    try { await s.RunAsync("Отмена", TimeSpan.FromSeconds(5), cts.Token); }
    catch (OperationCanceledException) { cancelled = true; }
}
Check(cancelled, "кооперативная отмена RunAsync");


var timedOut = false;
try { await s.RunWithTimeoutAsync("Тайм-аут", TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(300)); }
catch (TimeoutException) { timedOut = true; }
Check(timedOut, "тайм-аут --> TimeoutException");


var externalCancel = false;
using (var cts = new CancellationTokenSource(100))
{
    try { await s.RunWithTimeoutAsync("Внешняя", TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10), cts.Token); }
    catch (TimeoutException) { }
    catch (OperationCanceledException) { externalCancel = true; }
}
Check(externalCancel, "внешняя отмена не TimeoutException");


var emptyName = false;
try { await s.RunAsync("  ", TimeSpan.FromMilliseconds(20)); }
catch (ArgumentException) { emptyName = true; }
Check(emptyName, "пустое имя --> чемодан вокзал на***");


var negative = false;
try { await s.RunAsync("Тест", TimeSpan.FromMilliseconds(-10)); }
catch (ArgumentOutOfRangeException) { negative = true; }
Check(negative, "отрицательная длительность --> чемодан вокзал на***");


var cancelledResult = await s.SafeAssync_hehe("Отмена", TimeSpan.FromSeconds(5), new CancellationTokenSource(100).Token);
Check(cancelledResult.Status == DiagnosticStatus.Cancelled, "Вернулся Cancelled вместо исключения");


var failedResult = await s.SafeAssync_hehe("  ", TimeSpan.FromMilliseconds(10));
Check(failedResult.Status == DiagnosticStatus.Failed && failedResult.Details.Contains("ArgumentException"), "Вернулся Failed с ошибкой(тектс)");


var seen = new List<DiagnosticStatus>();
var reported = await s.NotSafeAssync_not_hehe("Статусы", TimeSpan.FromMilliseconds(50), st => seen.Add(st));
Check(reported.Status == DiagnosticStatus.Completed && seen.SequenceEqual(new[] { DiagnosticStatus.Pending, DiagnosticStatus.Running, DiagnosticStatus.Completed }), "статусы Pending -> Running -> Completed");


var cpu = await s.Cpuha_Assync("ЦП", 100_000);
Check(cpu.Status == DiagnosticStatus.Completed && cpu.Details.StartsWith("Сумма"), "CPU через Task.Run");
Console.ReadLine();
Console.Clear();
Console.ForegroundColor = ConsoleColor.Red;
Console.WriteLine("FAIL - одна диагностика завершается \nFAIL - WhenAll ждёт три диагностики \nFAIL - WhenAny возвращает первую завершившуюся \nFAIL - кооперативная отмена RunAsync \nFAIL - тайм - аут-- > TimeoutException \nFAIL - внешняя отмена не TimeoutException \nFAIL - пустое имя-- > чемодан вокзал на *** \nFAIL - отрицательная длительность-- > чемодан вокзал на *** \nFAIL - Вернулся Cancelled вместо исключения \nFAIL - Вернулся Failed с ошибкой(тектс) \nFAIL - статусы Pending->Running->Completed \nFAIL - CPU через Task.Run");
Console.ReadLine();
Console.WriteLine("Я устал все давай бб");
Console.ReadLine();
Console.WriteLine($"Проваленно проверок: {passed} из {count}.");
Console.ResetColor();
Console.ReadLine();
Console.Clear();
Console.ForegroundColor = ConsoleColor.DarkYellow;
Console.WriteLine("Да ладно");
Console.ReadLine();
Console.Clear();
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Я пошутил");
Console.ReadLine();
Console.Clear();
Console.ResetColor();
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("PASS - одна диагностика завершается \nPASS - WhenAll ждёт три диагностики \nPASS - WhenAny возвращает первую завершившуюся \nPASS - кооперативная отмена RunAsync \nPASS - тайм - аут-- > TimeoutException \nPASS - внешняя отмена не TimeoutException \nPASS - пустое имя-- > чемодан вокзал на *** \nPASS - отрицательная длительность-- > чемодан вокзал на *** \nPASS - Вернулся Cancelled вместо исключения \nPASS - Вернулся Failed с ошибкой(тектс) \nPASS - статусы Pending->Running->Completed \nPASS - CPU через Task.Run");
Console.ResetColor();
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine($"Пройденно проверок: {passed} из {count}.");
Console.ResetColor();
Console.ForegroundColor = ConsoleColor.Red;
Console.WriteLine($"Проваленно проверок: {failed.Count} из {count}.");
Console.ResetColor();
// ================= ЭПИЛОГ =================
Console.ReadLine();
Console.Clear();
Console.Title = "Практика 06 — всё под контролем";
Console.ForegroundColor = ConsoleColor.DarkGray;
Console.WriteLine("P.S. Ни одна диагностика не пострадала.");
Console.ResetColor();
Console.ReadLine();

Console.Write("Сохраняю результат в журнал");
for (int i = 0; i < 3; i++) { Console.Write("."); await Task.Delay(600); }
Console.WriteLine(" готово");
Console.ReadLine();
Console.ForegroundColor = ConsoleColor.DarkGray;
Console.WriteLine("Журнал не найден. Сохранил в сердце.");
Console.ResetColor();
Console.ReadLine();

Console.Write("Удаляю obj и bin");
for (int i = 0; i < 4; i++) { Console.Write("."); await Task.Delay(500); }
Console.WriteLine("  готово");
await Task.Delay(500);
Console.ForegroundColor = ConsoleColor.Red;
Console.WriteLine("ШУЧУ. Не удаляю.");
Console.ResetColor();
Console.ReadLine();

Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("\nКстати, почему тайм-аут падал?");
Console.ResetColor();
await Task.Delay(700);
Console.WriteLine("В RunAsync уходил token, а не linked.Token.");
await Task.Delay(700);
Console.WriteLine("Целый вечер из-за одного слова. Бывает.");
Console.ReadLine();

Console.Clear();
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("Последний тест: тест на терпение.");
Console.ResetColor();
for (int i = 1; i <= 5; i++)
{
    Console.WriteLine($"  [{i}/5] нажмите Enter, чтобы продолжить");
    Console.ReadLine();
}
Console.WriteLine("  [6/6] хватит жать Enter");
Console.ReadLine();

Console.Clear();
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine(
@"      (^_^)
     /|_|
      | |
     /   \
");
Console.ResetColor();
if (failed.Count > 0)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Всё, бб. Но {failed.Count} из {count} реально упало: {string.Join(", ", failed)}");
}
else
{
    Console.WriteLine($"Всё, бб. {passed} из {count}, честно.");
}
Console.ResetColor();
Console.ReadLine();