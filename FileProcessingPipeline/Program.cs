using System.Text.Json;
using FileProcessingPipeline.Models;
using FileProcessingPipeline.Services;

namespace FileProcessingPipeline;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("================================");
        Console.WriteLine("     FILE PROCESSING PIPELINE");
        Console.WriteLine("================================");

        string dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");

        Directory.CreateDirectory(dataDirectory);

        string[] paths = Directory.GetFiles(dataDirectory, "*.txt");

        if (paths.Length == 0)
        {
            Console.WriteLine("Нет файлов для обработки.");
            return;
        }

        var tasks = paths
            .Select((path, index) => new FileTask(index + 1, path))
            .ToList();

        Console.WriteLine($"Найдено файлов: {tasks.Count}");

        AnalysisReport? report = args.Length > 0
            ? await RunFromArgumentsAsync(args, tasks)
            : await RunInteractiveAsync(tasks);

        if (report is null) { return;    }

        PrintReport(report);

        if (report.Mode.StartsWith("Pipeline", StringComparison.Ordinal)) { PrintWorkerStatistics(report); }

        await SaveReportAsync(report);
    }

    // Неинтерактивный запуск: удобно проверять конфигурации и сравнивать режимы из командной строки.
    private static async Task<AnalysisReport?> RunFromArgumentsAsync(
        string[] args,
        List<FileTask> tasks)
    {
        if (args[0] is "--seq")
        {
            return await SequentialProcessor.ProcessAsync(tasks);
        }

        if (args[0] is "--workers"
            && args.Length > 1
            && int.TryParse(args[1], out int workerCount)
            && workerCount > 0)
        {
            return await PipelineProcessor.ProcessAsync(tasks, workerCount);
        }

        // Сценарий с недоступным файлом: ошибка одного задания не должна останавливать обработку остальных и ронять программу.
        if (args[0] is "--errors")
        {
            int workers = args.Length > 1 &&
                int.TryParse(args[1], out int parsedWorkers) &&
                parsedWorkers > 0
                    ? parsedWorkers
                    : 2;

            var tasksWithError = new List<FileTask>(tasks)
            {
                new(
                    tasks.Count + 1,
                    Path.Combine(
                        AppContext.BaseDirectory,
                        "Data",
                        "missing.txt"))
            };

            return await PipelineProcessor.ProcessAsync(tasksWithError, workers);
        }

        if (args[0] is "--compare")
        {
            return await RunComparisonAsync(tasks);
        }

        Console.WriteLine("Использование: --seq | --workers N");
        return null;
    }

    private static async Task<AnalysisReport?> RunInteractiveAsync(
        List<FileTask> tasks)
    {
        Console.WriteLine();
        Console.WriteLine("1. Последовательная обработка");
        Console.WriteLine("2. Параллельная обработка");

        Console.Write("Выберите режим: ");

        string? choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                return await SequentialProcessor.ProcessAsync(tasks);

            case "2":
                Console.Write("Количество обработчиков: ");

                if (!int.TryParse(Console.ReadLine(), out int workerCount) ||
                    workerCount < 1)
                {
                    Console.WriteLine("Некорректное значение.");
                    return null;
                }

                return await PipelineProcessor.ProcessAsync(tasks, workerCount);

            default:
                Console.WriteLine("Неизвестный режим.");
                return null;
        }
    }

    private static async Task<AnalysisReport> RunComparisonAsync(
        List<FileTask> tasks)
    {
        Console.WriteLine();
        Console.WriteLine(
            "Режим            | Обработчики | Время, мс | Успешно | Совпадает");
        Console.WriteLine(
            "----------------------------------------------------------------");

        var sequential = await SequentialProcessor.ProcessAsync(
            tasks,
            logProgress: false);

        PrintComparisonRow("Sequential", sequential, same: true);

        foreach (int workers in new[] { 1, 2, 4, 8 })
        {
            var parallel = await PipelineProcessor.ProcessAsync(
                tasks,
                workers,
                logProgress: false);

            bool same = parallel.TotalLines == sequential.TotalLines
                && parallel.TotalWords == sequential.TotalWords
                && parallel.TotalCharacters == sequential.TotalCharacters
                && parallel.SuccessfulFiles == sequential.SuccessfulFiles;

            PrintComparisonRow($"Pipeline ({workers})", parallel, same);
        }

        return sequential;
    }

    private static void PrintComparisonRow(
        string mode,
        AnalysisReport report,
        bool same)
    {
        Console.WriteLine(
            $"{mode,-16} | {report.WorkerCount,11} | " +
            $"{report.ProcessingTimeMs,9} | {report.SuccessfulFiles,7} | " +
            $"{(same ? "да" : "НЕТ")}");
    }

    private static void PrintReport(AnalysisReport report)
    {
        Console.WriteLine();
        Console.WriteLine("========== ОТЧЁТ ==========");
        Console.WriteLine($"Режим: {report.Mode}");
        Console.WriteLine($"Всего файлов: {report.TotalFiles}");
        Console.WriteLine($"Успешно: {report.SuccessfulFiles}");
        Console.WriteLine($"Ошибок: {report.FailedFiles}");
        Console.WriteLine($"Обработчиков: {report.WorkerCount}");
        Console.WriteLine($"Строк: {report.TotalLines}");
        Console.WriteLine($"Слов: {report.TotalWords}");
        Console.WriteLine($"Символов: {report.TotalCharacters}");
        Console.WriteLine($"Время: {report.ProcessingTimeMs} мс");
        Console.WriteLine();
        Console.WriteLine("РЕЗУЛЬТАТЫ:");

        foreach (var result in report.Results)
        {
            Console.WriteLine(
                $"{result.FileName} | " +
                $"Worker: {result.WorkerId} | " +
                $"Строк: {result.Lines} | " +
                $"Слов: {result.Words}");
        }

        if (report.Errors.Count > 0)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ОШИБКИ:");
            Console.ResetColor();

            foreach (var error in report.Errors)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine( $"{error.FileName}: {error.ErrorType} - {error.Message}");
                Console.ResetColor();
            }
        }
    }

    // Сколько заданий выполнил каждый обработчик: видно, насколько равномерно распределилась нагрузка.
    private static void PrintWorkerStatistics(AnalysisReport report)
    {
        Console.WriteLine();
        Console.WriteLine("========== СТАТА WORKERS ==========");
        Console.WriteLine();

        foreach (var group in report.Results
                     .GroupBy(x => x.WorkerId)
                     .OrderBy(group => group.Key))
        {
            Console.WriteLine($"Worker {group.Key}: {group.Count()} заданий");
        }

        Console.WriteLine();
        Console.WriteLine($"Всего обработано: {report.Results.Count}");
        Console.WriteLine();
        Console.WriteLine("=======================================");
    }

    private static async Task SaveReportAsync(AnalysisReport report)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };

        string json = JsonSerializer.Serialize(report, options);

        string path = Path.Combine(AppContext.BaseDirectory, "report.json");

        await File.WriteAllTextAsync(path, json);

        Console.WriteLine();
        Console.WriteLine($"Отчёт сохранён: {path}");
    }
}
