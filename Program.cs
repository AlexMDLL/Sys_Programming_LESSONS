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

    //размеры спрашиваем в КБ, внутрь передаём байты
    long minBytes = ReadSizeInKb("МИН РАЗМЕР В КБ (жмаешь enter — я откисну, и дальше сам будешь): ", 0);

    long maxBytes;

    while (true)
    {
        maxBytes = ReadSizeInKb("МАКС РАЗМЕР В КБ (жмаешь enter — я откисну, и дальше сам будешь): ", long.MaxValue);

        if (maxBytes >= minBytes)
            break;

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Максимум меньше минимума, ты че, наоборот не?.");
        Console.ResetColor();
    }

    int maxConcurrency = ReadMaxConcurrency();

    var options = new ScanOptions(root, extensions, minBytes, maxBytes, maxConcurrency);

    var scanner = new FileScanner(new FileProbe());

    using var cts = new CancellationTokenSource();

    Console.WriteLine();
    Console.WriteLine("Начало сканирования положено.");
    Console.WriteLine("Нажмите Enter для сноса MBR.");

    Task<ScanSummary> scanTask = scanner.ScanAsync(options, cts.Token);

    // Ждём либо конца сканирования, либо нажатия Enter.
    Task inputTask = Task.Run(() => Console.ReadLine());

    if (await Task.WhenAny(scanTask, inputTask) == inputTask)
    {
        cts.Cancel();
        Console.WriteLine("Отмена...");
    }

    ScanSummary summary = await scanTask;

    PrintSummary(summary, maxConcurrency);
}

static void PrintSummary(ScanSummary summary, int maxConcurrency)
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
    Console.WriteLine();

    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("=== РЕЗУЛЬТАТ СКАНИРОВАНИЯ ===");
    Console.ResetColor();
    Console.WriteLine();
    Console.WriteLine($"Просмотрено файлов: {summary.ScannedFiles}");
    Console.WriteLine($"Подходящих файлов: {summary.MatchedFiles}");

    //в байтах
    string total = summary.TotalMatchedBytes < 1024
        ? $"{summary.TotalMatchedBytes} байт"
        : $"{summary.TotalMatchedBytes} байт ({FormatSize(summary.TotalMatchedBytes)})";

    Console.WriteLine($"Общий размер: {total}");
    Console.WriteLine($"Ошибок: {summary.Errors.Count}");
    Console.WriteLine($"Время выполнения: {summary.Duration.TotalSeconds.ToString("F2", CultureInfo.InvariantCulture)} сек.");
    Console.WriteLine();
    Console.WriteLine($"Ограничение: {maxConcurrency}");
    Console.WriteLine($"Максимум одновременно обрабатываемых файлов: {summary.MaxObservedConcurrency}");

    if (summary.Errors.Count > 0)
    {
        Console.WriteLine();
        Console.WriteLine("Ошибки:");

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

//размер в КБ, пустая строка = без ограничения
static long ReadSizeInKb(string prompt, long defaultBytes)
{
    while (true)
    {
        Console.Write(prompt);
        string? input = Console.ReadLine();

        if (input is null)
            throw new InvalidOperationException("Я отменил твой ввод данных.");

        input = input.Trim();

        if (input.Length == 0)
            return defaultBytes;

        if (long.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out long kb)
            && kb >= 0
            && kb <= long.MaxValue / 1024)
        {
            return kb * 1024;
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Мне нужно число КБ, а не вот это вот.");
        Console.ResetColor();
    }
}

//сколько файлов жрём одновременно, Enter = 4
static int ReadMaxConcurrency()
{
    while (true)
    {
        Console.Write("СКОЛЬКО ФАЙЛОВ ОДНОВРЕМЕННО ЖРАТЬ (Enter — 4): ");
        string? input = Console.ReadLine();

        if (input is null)
            throw new InvalidOperationException("Я отменил твой ввод данных.");

        input = input.Trim();

        if (input.Length == 0)
            return 4;

        if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value)
            && value >= 1
            && value <= 64)
        {
            return value;
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("От 1 до 64, я не бессмертный.");
        Console.ResetColor();
    }
}

//байты в КБ/МБ/ГБ, чтоб 13107200 не пугало
static string FormatSize(long bytes)
{
    string[] units = ["байт", "КБ", "МБ", "ГБ", "ТБ"];

    double value = bytes;
    int unit = 0;

    while (value >= 1024 && unit < units.Length - 1)
    {
        value /= 1024;
        unit++;
    }

    return unit == 0
        ? $"{bytes} байт"
        : $"{value.ToString("0.##", CultureInfo.InvariantCulture)} {units[unit]}";
}
