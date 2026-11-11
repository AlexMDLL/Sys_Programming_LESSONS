using System.ComponentModel;
using System.Diagnostics;
using System.Text;

ProcessStartInfo startInfo = new()
{
    FileName = "dotnet",
    UseShellExecute = false,
    CreateNoWindow = true,
    WorkingDirectory = Environment.CurrentDirectory,

    RedirectStandardOutput = true,  //Тут для второго
    RedirectStandardError = true    //Тут для второго
};

startInfo.ArgumentList.Add("--info");

using Process process = new()
{
    StartInfo = startInfo
};

string logPath = Path.Combine(AppContext.BaseDirectory, "process-log.txt");
var timeout = TimeSpan.FromSeconds(10);

string arguments = string.Join(" ", startInfo.ArgumentList);
DateTime startedAt = DateTime.Now;
var stopwatch = Stopwatch.StartNew();

int? pid = null;
int? exitCode = null;
string? errorText = null;
string? timeoutText = null;

try
{
    if (!process.Start())
    {
        Console.WriteLine("Не удалось запустить процесс.");
        return;
    }

    pid = process.Id;
    Console.WriteLine($"PID: {pid}");

    Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync();  //Тут для второго
    Task<string> stderrTask = process.StandardError.ReadToEndAsync();   //Тут для второго

    using var cts = new CancellationTokenSource(timeout);

    try
    {
        await process.WaitForExitAsync(cts.Token);
    }
    catch (OperationCanceledException)
    {
        // Отмена ожидания процесс не убивает — дерево гасим сами.
        timeoutText = $"превышено время выполнения ({timeout.TotalSeconds:0} с)";
        process.Kill(entireProcessTree: true);
        await process.WaitForExitAsync();
    }

    string stdout = await stdoutTask;   //Тут для второго
    string stderr = await stderrTask;   //Тут для второго

    stopwatch.Stop();
    exitCode = process.ExitCode;

    Console.WriteLine($"Длительность: {stopwatch.Elapsed}");
    Console.WriteLine($"Exit code: {exitCode}");

    if (timeoutText is not null)
    {
        Console.WriteLine($"Внимание: {timeoutText}.");
    }

    Console.WriteLine("=== STDOUT ===");    //Тут для второго
    Console.WriteLine(stdout);              //Тут для второго
    Console.WriteLine("=== STDERR ===");    //Тут для второго
    Console.WriteLine(stderr);              //Тут для второго
}
catch (Win32Exception ex)
{
    stopwatch.Stop();
    errorText = ex.Message;
    Console.WriteLine($"Ошибка запуска: {ex.Message}");
}
finally
{
    // Журнал пишем всегда: успех, ошибка запуска, тайм-аут.
    var log = new StringBuilder();
    log.AppendLine($"[{startedAt:yyyy-MM-dd HH:mm:ss}]");
    log.AppendLine($"Program: {startInfo.FileName}");
    log.AppendLine($"Arguments: {arguments}");

    if (pid is not null)
    {
        log.AppendLine($"PID: {pid}");
    }

    log.AppendLine($"Duration: {stopwatch.Elapsed}");

    if (exitCode is not null)
    {
        log.AppendLine($"ExitCode: {exitCode}");
    }

    if (errorText is not null)
    {
        log.AppendLine($"Error: {errorText}");
    }

    if (timeoutText is not null)
    {
        log.AppendLine($"Timeout: {timeoutText}");
    }

    log.AppendLine("--------------------------------");

    File.AppendAllText(logPath, log.ToString());
}
