using System.Collections.Concurrent;
using System.Diagnostics;
using AsyncFileScanner.Models;

namespace AsyncFileScanner.Services;

/**
 * Тут всё хитрее, чем в первой серии.
 * 1. файлы перечисляем через `Directory.EnumerateFiles` — он синхронный, сколько его не оборачивай в `async`.
 * 2. зато читаем их асинхронно и не по очереди, а пачкой.
 * 3. сколько файлов жрать одновременно — решает `SemaphoreSlim`, чтобы винда не легла.
 * 4. ошибка одного файла не роняет всё сканирование, она просто летит в список ошибок.
 * 5. отмена проверяется и при перечислении, и внутри `ReadAsync`.
 * Важно: задачи создаются сразу на все файлы, семафор ограничивает только одновременность.
 * Для миллионов файлов лучше `Channel<T>`, но у нас лаба, а не телеграм.
 */
public sealed class FileScanner
{
    private readonly FileProbe _probe;

    public FileScanner(FileProbe probe) { _probe = probe; }

    public async Task<ScanSummary> ScanAsync(
        ScanOptions options,
        CancellationToken cancellationToken)
    {
        ValidateOptions(options);

        var stopwatch = Stopwatch.StartNew();
        var results = new ConcurrentBag<ScanResult>();   //сюда кидаем находки из разных потоков
        var errors = new ConcurrentBag<string>();
        var state = new ScanState();
        using var semaphore = new SemaphoreSlim(options.MaxConcurrency, options.MaxConcurrency);

        bool cancelled = false;
        try
        {
            List<string> files = EnumerateFiles(options, errors, cancellationToken);

            //на каждый файл заводим задачу 
            Task<ScanResult?>[] tasks = files
                .Select(path => ProcessFileLimitedAsync(
                    path,
                    options,
                    semaphore,
                    state,
                    results,
                    errors,
                    cancellationToken))
                .ToArray();

            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            // ГАЛЯ ОТМЕНАА!! тут короче отмена и вывод сканера то шо он успел
            cancelled = true;
        }
        stopwatch.Stop();

        return CreateSummary(results, errors, state, stopwatch.Elapsed, cancelled);
    }
    private async Task<ScanResult?> ProcessFileLimitedAsync(
        string path,
        ScanOptions options,
        SemaphoreSlim semaphore,
        ScanState state,
        ConcurrentBag<ScanResult> results,
        ConcurrentBag<string> errors,
        CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken);

        state.OperationStarted();
        await Task.Yield();

        try
        {
            ScanResult? result = await ProcessFileAsync(path, options, state, errors, cancellationToken);

            //пишем сразу, чтоб при отмене не потерять то, что уже успели
            if (result is not null)
                results.Add(result);

            return result;
        }
        finally
        {
            state.OperationFinished();
            semaphore.Release();
        }
    }

    private async Task<ScanResult?> ProcessFileAsync(
        string path,
        ScanOptions options,
        ScanState state,
        ConcurrentBag<string> errors,
        CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var file = new FileInfo(path);
            if (!file.Exists)
                return null;

            //файл посмотрели, даже если он не пролезет по размеру
            state.FileScanned();

            //фильтр по размеру: FileInfo.Length всегда в байтах
            if (file.Length < options.MinBytes || file.Length > options.MaxBytes)
                return null;

            // читаем 1024 байта из начала файла с assynk чтобы прога не была невдупленышем
            int bytesRead = await _probe.ReadHeaderAsync( path, cancellationToken);

            return new ScanResult(
                file.Name,
                file.FullName,
                file.Length,
                bytesRead);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            // Нет прав, файл занят, ошибка ввода-вывода — фиксируем и идём дальше.
            errors.Add($"{path}: {ex.Message}");
            return null;
        }
    }

    private static List<string> EnumerateFiles(
        ScanOptions options,
        ConcurrentBag<string> errors,
        CancellationToken cancellationToken)
    {
        var files = new List<string>();

        var enumerationOptions = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
            // Чтобы отойти от бесконечного бекрумса
            AttributesToSkip = FileAttributes.ReparsePoint,
            ReturnSpecialDirectories = false
        };

        try
        {
            foreach (string path in Directory.EnumerateFiles(
                         options.RootPath,
                         "*",
                         enumerationOptions))
            {
                cancellationToken.ThrowIfCancellationRequested();

                string extension = Path.GetExtension(path);
                if (options.Extensions.Contains(extension))
                    files.Add(path);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { errors.Add($"Не читабельно, меняй: {ex.Message}"); }
        return files;
    }

    //из говна и палок в норм список + итоги, чтобы вывод был good
    private static ScanSummary CreateSummary(
        ConcurrentBag<ScanResult> results,
        ConcurrentBag<string> errors,
        ScanState state,
        TimeSpan duration,
        bool cancelled)
    {
        List<ScanResult> ordered = results
            .OrderBy(r => r.FileName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(r => r.FullPath, StringComparer.OrdinalIgnoreCase)
            .ToList();

        long totalBytes = 0;
        foreach (ScanResult result in ordered)
            totalBytes += result.SizeBytes;

        return new ScanSummary(
            state.Scanned,
            ordered.Count,
            totalBytes,
            duration,
            ordered,
            errors.OrderBy(e => e, StringComparer.OrdinalIgnoreCase).ToList(),
            state.MaxObservedOperations,
            cancelled);
    }

    private static void ValidateOptions(ScanOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.RootPath)) 
            throw new ArgumentException("Я че сам путь искать должен?.");

        if (!Directory.Exists(options.RootPath))
            throw new DirectoryNotFoundException($"Каталога нема: {options.RootPath}");

        if (options.Extensions is null || options.Extensions.Count == 0)
            throw new ArgumentException("И по каким расширениям мне искать?.");

        if (options.MinBytes < 0)
            throw new ArgumentOutOfRangeException(nameof(options), "Минус размер? Серьёзно?.");

        if (options.MaxBytes < options.MinBytes)
            throw new ArgumentException("Максимум меньше минимума, ты че, наоборот не?.");

        if (options.MaxConcurrency < 1)
            throw new ArgumentOutOfRangeException(nameof(options), "Одновременно и ноль файлов? Так не бывает.");
    }

    //счётчики, которые пинают все задачи разом
    private sealed class ScanState
    {
        private int _scanned;
        private int _active;
        private int _maxObserved;

        public int Scanned => Volatile.Read(ref _scanned);

        public int MaxObservedOperations => Volatile.Read(ref _maxObserved);

        public void FileScanned() => Interlocked.Increment(ref _scanned);

        public void OperationStarted()
        {
            int active = Interlocked.Increment(ref _active);

            int observed = Volatile.Read(ref _maxObserved);

            while (active > observed)
            {
                int previous = Interlocked.CompareExchange(
                    ref _maxObserved,
                    active,
                    observed);

                if (previous == observed)
                    break;

                observed = previous;
            }
        }

        public void OperationFinished() => Interlocked.Decrement(ref _active);
    }
}
