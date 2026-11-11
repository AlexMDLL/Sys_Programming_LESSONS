using System.Collections.Concurrent;
using System.Diagnostics;

Order[] orders =
[
    new(1, 100),
    new(2, 200),
    new(3, 300),
    new(4, 400),
    new(5, 500),
    new(6, 600),
    new(7, 700),
    new(8, 800)
];

// Демо: разок глазами смотрим, как оно работает.
Console.WriteLine("=== Демо: последовательно ===");
RunSequential(orders, verbose: true);

Console.WriteLine();
Console.WriteLine("=== Демо: многопоточно ===");
var demoErrors = new ConcurrentQueue<string>();
RunParallel(orders, demoErrors, verbose: true);

// hw: пять замеров последовательной и многопоточной версий.
Console.WriteLine();
Console.WriteLine("=== 5 запусков ===");
Console.WriteLine("№ | Последовательно, мс | Многопоточно, мс | Быстрее");
Console.WriteLine("--+----------------------+------------------+------------------------");

double seqSum = 0;
double parSum = 0;

for (int run = 1; run <= 5; run++)
{
    var errors = new ConcurrentQueue<string>();

    double seq = RunSequential(orders).TotalMilliseconds;
    double par = RunParallel(orders, errors).TotalMilliseconds;

    seqSum += seq;
    parSum += par;

    string faster = par < seq ? "многопоточно" : "последовательно";
    Console.WriteLine($"{run} | {seq,20:F0} | {par,16:F0} | {faster}");
}

Console.WriteLine();
Console.WriteLine($"В среднем: последовательно {seqSum / 5:F0} мс, многопоточно {parSum / 5:F0} мс.");

// hw: ошибка одного потока не должна ронять остальные и теряться.
Console.WriteLine();
Console.WriteLine("=== Ошибки в рабочих потоках ===");

Order[] withBad = [.. orders, new(9, -100)];
var badErrors = new ConcurrentQueue<string>();

RunParallel(withBad, badErrors);

foreach (string error in badErrors)
{
    Console.WriteLine($"ОШИБКА: {error}");
}

Console.WriteLine($"Всего ошибок: {badErrors.Count}");

TimeSpan RunSequential(Order[] items, bool verbose = false)
{
    var sw = Stopwatch.StartNew();

    foreach (Order order in items)
    {
        pocOrda(order, verbose);
    }

    sw.Stop();
    return sw.Elapsed;
}

TimeSpan RunParallel(Order[] items, ConcurrentQueue<string> errors, bool verbose = false)
{
    var sw = Stopwatch.StartNew();
    Thread[] threads = new Thread[items.Length];

    for (int i = 0; i < items.Length; i++)
    {
        Order order = items[i];

        threads[i] = new Thread(() =>
        {
            try
            {
                pocOrda(order, verbose);
            }
            catch (Exception ex)
            {
                // Не пустой catch: ошибку складываем в общую коллекцию.
                errors.Enqueue($"Заказ {order.Id}: {ex.Message}");
            }
        });

        threads[i].Start();
    }

    foreach (Thread thread in threads) { thread.Join(); }

    sw.Stop();
    return sw.Elapsed;
}

void pocOrda(Order order, bool verbose)
{
    if (order.Amount <= 0)
    {
        throw new ArgumentOutOfRangeException(
            nameof(order), "сумма заказа должна быть положительной");
    }

    if (verbose)
    {
        Console.WriteLine($"Заказ {order.Id}: поток {Environment.CurrentManagedThreadId}");
    }

    Thread.Sleep(100);

    decimal total = order.Amount * 1.05m;

    if (verbose)
    {
        Console.WriteLine($"Заказ {order.Id} завершён. Сумма: {total:F2}");
    }
}

// Ответы на вопросы из домашки:
// 1. Да, цифры между запусками пляшут: планировщик, прогрев JIT, фоновая нагрузка.
// 2. Нет, не всегда: на мелких задачах потоки проигрывают из-за накладных расходов.
// 3. С реальными вычислениями Sleep уходит, потоки начинают делить ядра —
//    ускорение упирается в число ядер, а не в число потоков.
// 4. Поток — это стек (~1 МБ), создание и переключение; если работы меньше
//    этих затрат, дешевле обычный цикл.

public record Order(int Id, decimal Amount);
