using System.Collections.Concurrent;
using FileProcessingPipeline.Models;

namespace FileProcessingPipeline.Services;

// Центральный класс практического проекта.
// Здесь разделены три обязанности:
// 1. Создание заданий.
// 2. Выполнение заданий независимыми обработчиками.
// 3. Сбор результатов.
public static class PipelineProcessor
{
    public static async Task<AnalysisReport> ProcessAsync(
        IEnumerable<FileTask> tasks,
        int workerCount)
    {
        ArgumentNullException.ThrowIfNull(tasks);

        if (workerCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(workerCount));
        }

        var taskList = tasks.ToList();

        var results = new ConcurrentBag<FileAnalysisResult>();
        var errors = new ConcurrentBag<FileProcessingError>();

        // TODO 1 (П1): создайте канал, запишите в него все задания
        // и закройте запись.
        // TODO 2 (П1): организуйте получение заданий из канала в WorkerAsync.
        // TODO 3 (П1): запустите workerCount независимых обработчиков.
        // TODO 4 (П1): дождитесь завершения всех обработчиков.
        // TODO 5 (П1): измерьте общее время выполнения и передайте его в отчёт.

        await Task.Yield();

        throw new NotImplementedException(
            "Конвейер не реализован: " +
            $"{taskList.Count} заданий, " +
            $"{results.Count} результатов, " +
            $"{errors.Count} ошибок.");
    }

    // Обрабатывает одно задание, но пока не определено,
    // откуда обработчик будет получать следующее задание.
    private static async Task WorkerAsync(
        int workerId,
        FileTask task,
        ConcurrentBag<FileAnalysisResult> results,
        ConcurrentBag<FileProcessingError> errors)
    {
        try
        {
            Console.WriteLine($"[WORKER {workerId}] Начал: {task.FilePath}");

            var result = await FileAnalyzer.AnalyzeAsync(task, workerId);

            results.Add(result);

            Console.WriteLine($"[WORKER {workerId}] Завершил: {result.FileName}");
        }
        catch (IOException ex)
        {
            errors.Add(new FileProcessingError(
                task.Id,
                Path.GetFileName(task.FilePath),
                nameof(IOException),
                ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            errors.Add(new FileProcessingError(
                task.Id,
                Path.GetFileName(task.FilePath),
                nameof(UnauthorizedAccessException),
                ex.Message));
        }
    }
}
