using System.Diagnostics;
namespace TaskCoordinator;
public sealed class DiagnosticService
{
    public async Task<DiagnosticResult> RunAsync(string name, TimeSpan duration, CancellationToken token = default)
    {

        // TODO 1: Проверить имя и неотрицательную длительность.
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Имя диагностики обязательно.", nameof(name));
        if (duration < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(duration), "Длительность не может быть отрицательной.");
        var sw = Stopwatch.StartNew();

        // TODO 2: Передать token в Task.Delay.
        await Task.Delay(duration, token);
        sw.Stop();

        // TODO 3: Вернуть статус Completed и длительность.
        return new DiagnosticResult(name, DiagnosticStatus.Completed, "Завершено.", sw.Elapsed);
    }



    public async Task<DiagnosticResult> Cpuha_Assync(string name, int iterations, CancellationToken token = default)
    {
        // TODO 4: Перенести CPU-bound цикл в Task.Run; регулярно проверять token.
        var sw = Stopwatch.StartNew();
        var s = await Task.Run(() =>
        {
            long sum = 0;
        for (int i = 0; i < iterations; i++) 
        {
            if (i % 1000 == 0) token.ThrowIfCancellationRequested();
            sum += i % 7; 
        }
        return sum;
        }, token);
        sw.Stop();
        return new DiagnosticResult(name, DiagnosticStatus.Completed, $"Сумма: {s}", sw.Elapsed);
        //await Task.Yield(); 
        //return new(name, DiagnosticStatus.Completed, $"Сумма: {sum}", TimeSpan.Zero);
    }




    public async Task<IReadOnlyList<DiagnosticResult>> RunAllAsync(CancellationToken token = default)
    {
        var tasks = new[] { 
            RunAsync("Сеть", TimeSpan.FromMilliseconds(600), token), 
            RunAsync("Диск", TimeSpan.FromMilliseconds(900), token), 
            RunAsync("База", TimeSpan.FromMilliseconds(700), token) 
        };
        // TODO 5: Дождаться всех задач через WhenAll.
        var results = await Task.WhenAll(tasks);
        return results;
    }



    public async Task<DiagnosticResult> RunWithTimeoutAsync(string name, TimeSpan duration, TimeSpan timeout, CancellationToken token = default)
    {
        // TODO 6: Создать linked CancellationTokenSource на основе внешнего token.
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(token);

        // TODO 7: Настроить CancelAfter(timeout).
        linked.CancelAfter(timeout);

        // TODO 8: Отличить тайм-аут от отмены вызывающей стороны; тайм-аут сообщать TimeoutException.
        try { return await RunAsync(name, duration, linked.Token); }
        catch (OperationCanceledException ex) when (!token.IsCancellationRequested) {
            throw new TimeoutException($"Диагностика '{name}' не завершилась за {timeout.TotalSeconds:f} с.", ex); 
        }
    }



    public async Task<string> Kto_Perviy(CancellationToken token = default)// tot ***
    {
        var tasks = new[] { RunAsync("Быстрая", TimeSpan.FromMilliseconds(150), token), RunAsync("Средняя", TimeSpan.FromMilliseconds(350), token), RunAsync("Медленная", TimeSpan.FromMilliseconds(600), token) };
        // TODO 9: Использовать WhenAny, затем await первой задачи.
        //await Task.WhenAll(tasks);
        var first = await Task.WhenAny(tasks);
        var result = await first;
        await Task.WhenAll(tasks);
        //return "TODO";
        return result.Name;
    }
    // TODO 10: Добавить обёртку, которая возвращает статусы Failed/Cancelled
    // и текст ошибки вместо сокрытия исключений.
    public async Task<DiagnosticResult> SafeAssync_hehe(string name, TimeSpan duration, CancellationToken token = default)
    {
        var sw = Stopwatch.StartNew();
        try { return await RunAsync(name, duration, token); }
        catch (OperationCanceledException) when (token.IsCancellationRequested) {
            sw.Stop();
            return new DiagnosticResult(name, DiagnosticStatus.Cancelled, "Отменено.", sw.Elapsed);
        }
        catch (Exception ex)  {
            sw.Stop();
            return new DiagnosticResult(name, DiagnosticStatus.Failed, $"Ошибка {ex.GetType().Name}: {ex.Message}", sw.Elapsed);
        }
    }
    public async Task<DiagnosticResult> NotSafeAssync_not_hehe(string name, TimeSpan duration, Action<DiagnosticStatus> onStatus, CancellationToken token = default)
    {
        onStatus(DiagnosticStatus.Pending);
        onStatus(DiagnosticStatus.Running);
        var result = await SafeAssync_hehe(name, duration, token);
        onStatus(result.Status);
        return result;
    }
}
