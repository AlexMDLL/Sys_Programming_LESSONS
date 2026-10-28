using System.ComponentModel;
using System.Diagnostics;

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

var stopwatch = Stopwatch.StartNew();

try
{
    if (!process.Start())
    {
        Console.WriteLine("Не удалось запустить процесс.");
        return;
    }

    Console.WriteLine($"PID: {process.Id}");

    Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync();  //Тут для второго
    Task<string> stderrTask = process.StandardError.ReadToEndAsync();   //Тут для второго

    await process.WaitForExitAsync();

    string stdout = await stdoutTask;   //Тут для второго
    string stderr = await stderrTask;   //Тут для второго

    stopwatch.Stop();

    Console.WriteLine($"Длительность: {stopwatch.Elapsed}");
    Console.WriteLine($"Exit code: {process.ExitCode}");

    Console.WriteLine("=== STDOUT ===");    //Тут для второго
    Console.WriteLine(stdout);              //Тут для второго
    Console.WriteLine("=== STDERR ===");    //Тут для второго
    Console.WriteLine(stderr);              //Тут для второго
}
catch (Win32Exception ex)
{
    stopwatch.Stop();
    Console.WriteLine($"Ошибка запуска: {ex.Message}");
}