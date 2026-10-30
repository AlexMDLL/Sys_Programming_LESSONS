using System.Diagnostics;
namespace AsyncMonitor;
public sealed class CheckService
{
    public async Task<CheckResult> CheckServiceAsync(string name, TimeSpan delay, CancellationToken cancellationToken = default)
    {
        // TODO 1: Отклонить пустое имя проверки.
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Имя проверки обязательно.", nameof(name));
        // TODO 2: Отклонить отрицательную длительность через ArgumentOutOfRangeException.
        if (delay < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(delay), "Длительность не может быть отрицательной.");
        var sw = Stopwatch.StartNew();
        // TODO 3: Использовать await Task.Delay(delay, cancellationToken), а не Thread.Sleep.
        await Task.Delay(delay, cancellationToken);
        sw.Stop();
        // TODO 4: Вернуть корректный CheckResult.
        return new CheckResult(name, true, sw.Elapsed, "Проверка завершена успешно.");
    }
    public async Task<IReadOnlyList<CheckResult>> RunSequentialAsync(IReadOnlyList<(string Name, TimeSpan Delay)> checks, CancellationToken token = default)
    {
        // TODO 5: Выполнить проверки по одной, ожидая каждую через await.
        var results = new List<CheckResult>();
        foreach (var c in checks)
            results.Add(await CheckServiceAsync(c.Name, c.Delay, token));
        return results;
    }
    public async Task<IReadOnlyList<CheckResult>> RunParallelAsync(IReadOnlyList<(string Name, TimeSpan Delay)> checks, CancellationToken token = default)
    {
        // TODO 6: Сначала создать задачи для всех проверок.
        var tasks = checks.Select(c => CheckServiceAsync(c.Name, c.Delay, token)).ToArray();
        // TODO 7: Совместно дождаться завершения всех задач через Task.WhenAll.
        var results = await Task.WhenAll(tasks);
        return results;
    }
}
