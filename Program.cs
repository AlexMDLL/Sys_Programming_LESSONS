using System.Globalization;
using AsyncFileScanner.Models;
using AsyncFileScanner.Services;

try { await RunAsync(); }
//ловим все что есть а то они меня уже зае****
catch (Exception ex) when (ex is ArgumentException or DirectoryNotFoundException or IOException or UnauthorizedAccessException or InvalidOperationException)
{
    Console.WriteLine();
    Console.WriteLine($"Ошибка: {ex.Message}");
}

static async Task RunAsync()
{
    Console.WriteLine("=== FILE SCANNER ===");
    Console.WriteLine();

    string root = ReadDirectoryPath();
    HashSet<string> extensions = ReadExtensions();

    var options = new ScanOptions(root, extensions);

    var scanner = new FileScanner(new FileProbe());

    using var cts = new CancellationTokenSource();

    Console.WriteLine();
    Console.WriteLine("Начало сканирования положено.");
    Console.WriteLine("Нажмите Enter для отмены.");

    Task<ScanSummary> scanTask = scanner.ScanAsync(options, cts.Token);

    // Ждём либо конца сканирования, либо нажатия Enter.
    Task inputTask = Task.Run(() => Console.ReadLine());

    if (await Task.WhenAny(scanTask, inputTask) == inputTask)
    {
        cts.Cancel();
        Console.WriteLine("Отмена...");
    }

    ScanSummary summary = await scanTask;

    PrintSummary(summary);
}

static void PrintSummary(ScanSummary summary)
{
    Console.WriteLine();

    if (summary.Cancelled)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("За что отменил? ГАЛЯ У НАС ОТМЕНА.");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Обработано файлов: {summary.ScannedFiles}");
        Console.ResetColor();
        return;
    }

    if (summary.Results.Count == 0)
    {
        Console.WriteLine("Я не нашел файлы с этим расширением, я остался голодни:( .");
    }
    else
    {
        Console.WriteLine("То что я съем:3 :");
        Console.WriteLine();

        for (int i = 0; i < summary.Results.Count; i++)
        {
            ScanResult result = summary.Results[i];

            Console.WriteLine(
                $"[{i + 1}] {result.FileName,-30} " +
                $"{result.SizeBytes,12} байт " +
                $"(прочитано: {result.BytesRead})");
        }
    }

    Console.WriteLine();
    Console.WriteLine("Сканирование завершено.");
    Console.WriteLine();
    Console.WriteLine($"Всего найдено файлов: {summary.MatchedFiles}");
    Console.WriteLine($"Общий размер: {summary.TotalMatchedBytes} байт");
    Console.WriteLine($"Время: {summary.Duration.TotalSeconds.ToString("F2", CultureInfo.InvariantCulture)} сек.");

    if (summary.Errors.Count > 0)
    {
        Console.WriteLine();
        Console.WriteLine($"Ошибок: {summary.Errors.Count}");

        foreach (string error in summary.Errors)
            Console.WriteLine($"  {error}");
    }
}

static string ReadDirectoryPath()
{
    while (true)
    {
        Console.Write("ПУТЬ К ФАЙЛАМ МНЕ В КОНСОЛЬ БЫСТРО: ");
        string? input = Console.ReadLine();

        if (input is null)
            throw new InvalidOperationException("Я отменил твой ввод данных.");

        input = input.Trim().Trim('"');

        if (input.Length == 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Я не ИИ чтобы искать путь к файлам, я тебе щас винду крашну и потом сам мучайся со стертым MBR.");
            Console.ResetColor();
            continue;
        }

        if (!Directory.Exists(input))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Че? Где каталог? А, я его съел точно:3.");
            Console.ResetColor();
            continue;
        }

        return input;
    }
}

static HashSet<string> ReadExtensions()
{
    while (true)
    {
        Console.Write("РАСШИРЕНИЯ МНЕ В КОНСОЛЬ БЫСТРО (например, .cs,.txt,.json): ");
        string? input = Console.ReadLine();

        if (input is null)
            throw new InvalidOperationException("Я отменил твой ввод данных.");

        string[] parts = input
            .Split(',', StringSplitOptions.RemoveEmptyEntries |
                        StringSplitOptions.TrimEntries)
            .Select(x => x.StartsWith('.') ? x : "." + x)
            .ToArray();

        if (parts.Length == 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Как я без расширений должен что то искать??.");
            Console.ResetColor();
            continue;
        }

        return new HashSet<string>(parts, StringComparer.OrdinalIgnoreCase);
    }
}