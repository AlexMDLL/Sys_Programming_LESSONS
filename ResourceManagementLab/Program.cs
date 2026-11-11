using ResourceManagementLab.Models;
using ResourceManagementLab.Services;

namespace ResourceManagementLab;

internal class Program
{
    private static void Main()
    {
        string dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");
        Directory.CreateDirectory(dataDirectory);

        string filePath = Path.Combine(dataDirectory, "sample.txt");
        File.WriteAllText(filePath, "Hello from C#!\nResource management example.");

        var info = new ResourceInfo(Path.GetFileName(filePath), IsDisposed: false);

        Ui.Section("=== RESOURCE MANAGEMENT LAB ===");

        Ui.Step("1. Чтение без using");
        Ui.Line(FileReaderWithoutUsing.Read(filePath));

        // Разница простая: без using поток закроется когда-нибудь потом,
        // а может и не закрыться. С using — сразу на выходе из метода.
        Ui.Step("2. Чтение с using");
        RunStep(() => Ui.Line(FileReaderWithUsing.Read(filePath)));

        Ui.Step("2б. Чтение с ошибкой — using всё равно закроет поток");
        try { FileReaderWithUsing.ReadWithException(filePath); }
        catch (InvalidOperationException ex) { Ui.Error($"Перехвачено: {ex.Message}"); }

        Ui.Step("3. Проверка Dispose");
        RunStep(() =>
        {
            using var storage = new FileStorage(filePath);
            storage.Write("Данные записаны через FileStorage.");
            Ui.Line(storage.Read());
            Ui.Line($"Ресурс: {info.Name}");
        });

        Ui.Step("4. Проверка повторного Dispose");
        RunStep(() =>
        {
            var secondStorage = new FileStorage(filePath);
            secondStorage.Dispose();
            secondStorage.Dispose();
            Ui.Success("Повторный Dispose прошёл без ошибок.");
        });

        Ui.Step("5. Демонстрация GC");
        RunStep(MemoryDemo.Run);

        Ui.Success("Работа программы завершена.");
    }

    // Скоро
    private static void RunStep(Action action)
    {
        try
        {
            action();
        }
        catch (NotImplementedException ex)
        {
            Ui.Error($"Ещё не реализовано: {ex.Message}");
        }
    }
}
