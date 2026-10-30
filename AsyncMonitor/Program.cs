using AsyncMonitor;
using System;
var service = new CheckService();
var checks = new (string Name, TimeSpan Delay)[]
{
    ("Сеть", TimeSpan.FromMilliseconds(700)),
    ("База данных", TimeSpan.FromMilliseconds(900)),
    ("Файлы", TimeSpan.FromMilliseconds(500))
};
while (true)
{
    Console.WriteLine("\n1 — одна проверка; 2 — последовательно; 3 — параллельно; 4 — длительная с отменой; 0 — выход");
    var cmd = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(cmd))
        break;
    if (cmd == "0")
        break;
    try
    {
        if (cmd == "1")
        {
            // TODO 8: Вызвать метод с await, не применять .Wait() или .Result.
            var one = await service.CheckServiceAsync("Сеть", TimeSpan.FromMilliseconds(700));
            Print([one]);
        }
        else if (cmd == "2")
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var r = await service.RunSequentialAsync(checks);
            sw.Stop();
            Console.WriteLine($"Последовательно: {sw.ElapsedMilliseconds} мс");
            Print(r);
        }
        else if (cmd == "3")
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var r = await service.RunParallelAsync(checks);
            sw.Stop();
            Console.WriteLine($"Параллельно: {sw.ElapsedMilliseconds} мс");
            Print(r);
        }
        else if (cmd == "4")
        {
            using var cts = new CancellationTokenSource();
            var longTask = service.CheckServiceAsync("Долгая проверка", TimeSpan.FromSeconds(5), cts.Token);
            Console.WriteLine("Идёт проверка. Нажмите 'p' для отмены...\n");
            while (!longTask.IsCompleted)
            {
                if (Console.KeyAvailable && Console.ReadKey().KeyChar == 'p')
                {
                    cts.Cancel();
                    Console.Clear();
                    Console.WriteLine("Отмена...\n");
                    Console.ReadLine();
                    Console.Clear();

                }
                Console.Clear();
                Console.Write("...");
                await Task.Delay(200);
                
            }
            Print([await longTask]);
        }
        else
            Console.WriteLine("Неизвестная команда.");
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Проверка отменена.\n");
        Console.Clear();
    }
    catch (Exception ex)
    { // TODO 9: Не скрывать исключение; сообщить тип и причину.
        Console.WriteLine($"Ошибка {ex.GetType().Name}: {ex.Message}");
    }
}
// TODO 10: Добавить режим контроля/отмены длительной проверки и показать,
// что меню не зависает на ожидании Task.Delay.
static void Print(IEnumerable<CheckResult> xs)
{
    foreach (var x in xs)
    {
        var mark = x.Success ? "OK" : "FAIL";
        Console.WriteLine($"  [{mark}] {x.Name}: {x.Duration.TotalMilliseconds:F0} мс — {x.Message}");
    }
}
