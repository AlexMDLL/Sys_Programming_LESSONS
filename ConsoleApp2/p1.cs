using System.Diagnostics;

Order[] orders =
{
    new(1, 100),
    new(2, 200),
    new(3, 300),
    new(4, 400),
    new(5, 500),
    new(6, 600),
    new(7, 700),
    new(8, 800)
};

Console.WriteLine("=== Последовательная обработка ===");
var ochered = Stopwatch.StartNew();
foreach (Order order in orders) { pocOrda(order); }
ochered.Stop();
Console.WriteLine($"Время: {ochered.Elapsed}");

Console.WriteLine();
Console.WriteLine("=== Многопоточная обработка ===");
Thread[] threads = new Thread[orders.Length];
var potoki = Stopwatch.StartNew();

for (int i = 0; i < orders.Length; i++)
{
    Order order = orders[i];
    threads[i] = new Thread(() => pocOrda(order));
    threads[i].Start();
}

foreach (Thread thread in threads) { thread.Join(); }
potoki.Stop();
Console.WriteLine($"Время: {potoki.Elapsed}");

void pocOrda(Order order)
{
    Console.WriteLine($"Заказ {order.Id}: поток {Environment.CurrentManagedThreadId}");

    Thread.Sleep(100);

    decimal total = order.Amount * 1.05m;

    Console.WriteLine($"Заказ {order.Id} завершён. Сумма: {total:F2}");
}

public record Order(int Id, decimal Amount);