using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Channels;
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
        int workerCount,
        bool logProgress = true)
    {
        ArgumentNullException.ThrowIfNull(tasks);

        if (workerCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(workerCount));
        }

        var taskList = tasks.ToList();

        var results = new ConcurrentBag<FileAnalysisResult>();
        var errors = new ConcurrentBag<FileProcessingError>();

        var stopwatch = Stopwatch.StartNew();

        // Ограниченный канал создаёт обратное давление: производитель не может накопить больше заданий, чем успевают разобрать обработчики.
        var channel = Channel.CreateBounded<FileTask>(
            new BoundedChannelOptions(workerCount * 4)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleWriter = true,
                SingleReader = false
            });

        // Производитель работает отдельной задачей: пишет задания в канал и закрывает запись, когда задания закончатся.
        Task producer = Task.Run(async () =>
        {
            foreach (var task in taskList)
            {
                await channel.Writer.WriteAsync(task);
            }

            channel.Writer.TryComplete();
        });

        // Каждый обработчик — независимая задача, читающая из того же канала: свободный обработчик сразу забирает следующее задание из очереди.
        Task[] workers = Enumerable
            .Range(1, workerCount)
            .Select(workerId => WorkerAsync(
                workerId,
                channel.Reader,
                results,
                errors,
                logProgress))
            .ToArray();

        // Главный поток ждёт, пока производитель раздаст задания, а обработчики дочитают очередь и завершатся.
        await Task.WhenAll([.. workers, producer]);

        stopwatch.Stop();

        // Время измеряется от создания канала до завершения последнего обработчика — то есть вся работа конвейера целиком.
        return ReportService.Create(
            $"Pipeline ({workerCount} workers)",
            taskList.Count,
            results.ToList(),
            errors.ToList(),
            workerCount,
            stopwatch.ElapsedMilliseconds);
    }

    // Обработчик читает задания из канала, пока производитель не закроет запись и очередь не опустеет.
    // Ошибка одного файла не останавливает обработчик: она попадает в errors, и цикл продолжается.
    private static async Task WorkerAsync(
        int workerId,
        ChannelReader<FileTask> reader,
        ConcurrentBag<FileAnalysisResult> results,
        ConcurrentBag<FileProcessingError> errors,
        bool logProgress)
    {
        await foreach (var task in reader.ReadAllAsync())
        {
            try
            {
                if (logProgress)
                {
                    Console.WriteLine(
                        $"[WORKER {workerId}] Начал: {task.FilePath}");
                }

                var result = await FileAnalyzer.AnalyzeAsync(task, workerId);

                results.Add(result);

                if (logProgress)
                {
                    Console.WriteLine(
                        $"[WORKER {workerId}] Завершил: {result.FileName}");
                }
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
}
