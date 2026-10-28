using System;
using System.Threading;

public class P2
{
    private static int completedOrders = 0;

    public static void Main()
    {
        const int threadCount = 8;
        const int incrementsPerThread = 100_000;

        int completedOrders = 0;
        Thread[] threads = new Thread[threadCount];

        for (int i = 0; i < threadCount; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < incrementsPerThread; j++)
                {
                    Interlocked.Increment(ref completedOrders);
                    //completedOrders++;
                }
            });
        }

        foreach (Thread thread in threads) thread.Start();
        foreach (Thread thread in threads) thread.Join();

        int expected = threadCount * incrementsPerThread;
        Console.WriteLine($"Ожидалось: {expected}");
        Console.WriteLine($"Получено: {completedOrders}");
    }
}