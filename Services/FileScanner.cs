using System.Diagnostics;
using AsyncFileScanner.Models;

namespace AsyncFileScanner.Services;

/**
 * В этом примере есть несколько важных особенностей.
 * 1. перечисление файлов выполняется через `Directory.EnumerateFiles`. Это синхронный API. Он не становится асинхронным оттого, что находится внутри `async` метода.
 * 2. асинхронная операция здесь — чтение файла через `ReadHeaderAsync`.
 * 3. ошибки одного файла сохраняются в список. Они не приводят к остановке всего сканирования.
 * 4. отмена проверяется перед обработкой каждого следующего файла, а также передаётся непосредственно в `ReadAsync`.
 * Важно: отмена не может гарантированно мгновенно прервать системный вызов перечисления файлов. В данном варианте она проверяется между итерациями.
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
        var results = new List<ScanResult>();
        var errors = new List<string>();

        int scannedFiles = 0;
        long totalBytes = 0;
        bool cancelled = false;
        try
        {
            foreach (string path in EnumerateFiles(options, errors))
            {
                cancellationToken.ThrowIfCancellationRequested();
                scannedFiles++;
                try
                {
                    var file = new FileInfo(path);
                    if (!file.Exists)
                        continue;

                    // читаем 1024 байта из начала файла с assynk чтобы прога не была невдупленышем
                    int bytesRead = await _probe.ReadHeaderAsync( path, cancellationToken);

                    results.Add(new ScanResult(
                        file.Name,
                        file.FullName,
                        file.Length,
                        bytesRead));

                    totalBytes += file.Length;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    // Нет прав, файл занят, ошибка ввода-вывода — фиксируем и идём дальше.
                    errors.Add($"{path}: {ex.Message}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // ГАЛЯ ОТМЕНАА!! тут короче отмена и вывод сканера то шо он успел
            cancelled = true;
        }
        stopwatch.Stop();

        return new ScanSummary(
            scannedFiles,
            results.Count,
            totalBytes,
            stopwatch.Elapsed,
            results,
            errors,
            cancelled);
    }

    private static List<string> EnumerateFiles(
        ScanOptions options,
        List<string> errors)
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
                string extension = Path.GetExtension(path);
                if (options.Extensions.Contains(extension))
                    files.Add(path);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { errors.Add($"Не читабельно, меняй: {ex.Message}"); } return files;
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
    }
}