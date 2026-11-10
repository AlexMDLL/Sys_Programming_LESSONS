using FileProcessingPipeline.Models;
using FileProcessingPipeline.Services;

namespace FileProcessingPipeline.Tests;

public class PipelineProcessorTests
{
    [Fact]
    public async Task ProcessAsync_WithMissingFile_CollectsErrorAndProcessesOthers()
    {
        string directory = Path.Combine( Path.GetTempPath(), $"fpp-{Guid.NewGuid():N}");

        Directory.CreateDirectory(directory);
        try
        {
            string existing = Path.Combine(directory, "ok.txt");
            await File.WriteAllTextAsync(existing, "one two\nthree");
            var tasks = new List<FileTask>
            {
                new(1, existing),
                new(2, Path.Combine(directory, "missing.txt"))
            };

            var report = await PipelineProcessor.ProcessAsync(
                tasks,
                workerCount: 2);

            Assert.Equal(1, report.SuccessfulFiles);
            Assert.Equal(1, report.FailedFiles);
            Assert.Single(report.Errors);
            Assert.Equal("missing.txt", report.Errors[0].FileName);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task ProcessAsync_ProcessesEveryTaskExactlyOnce()
    {
        string directory = Path.Combine(
            Path.GetTempPath(),
            $"fpp-{Guid.NewGuid():N}");

        Directory.CreateDirectory(directory);

        try
        {
            var tasks = new List<FileTask>();

            for (int i = 1; i <= 10; i++)
            {
                string path = Path.Combine(directory, $"file{i}.txt");
                await File.WriteAllTextAsync(path, $"line {i}\nline {i + 1}");
                tasks.Add(new FileTask(i, path));
            }

            var report = await PipelineProcessor.ProcessAsync(
                tasks,
                workerCount: 4);

            Assert.Equal(10, report.SuccessfulFiles);
            Assert.Equal(0, report.FailedFiles);
            Assert.Equal( tasks.Select(x => x.Id).OrderBy(id => id),
                report.Results.Select(x => x.TaskId).OrderBy(id => id));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
