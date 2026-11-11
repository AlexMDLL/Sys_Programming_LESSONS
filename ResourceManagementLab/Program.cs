using ResourceManagementLab.Models;
using ResourceManagementLab.Services;

namespace ResourceManagementLab;

internal class Program
{
    private static async Task Main()
    {
        string dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");
        Directory.CreateDirectory(dataDirectory);

        string filePath = Path.Combine(dataDirectory, "sample.txt");
        File.WriteAllText(filePath, "Hello from C#!\nResource management example.");

        // Для записи держим отдельный файл: после шага 1 он ещё занят
        // утёкшим потоком, и FileShare.None на нём бы упал.
        string storagePath = Path.Combine(dataDirectory, "storage.txt");

        var info = new ResourceInfo(Path.GetFileName(storagePath), IsDisposed: false);

        Ui.Section("=== RESOURCE MANAGEMENT LAB ===");

        Ui.Step("1. Чтение без using");
        Ui.Line(FileReaderWithoutUsing.Read(filePath));
        // Поток так и остался открытым — в этом и проблема.

        // Без using поток закроется когда-нибудь потом, а может и не закрыться.
        // С using — сразу на выходе, даже если что-то упало.
        Ui.Step("2. Чтение с using");
        Ui.Line(FileReaderWithUsing.Read(filePath));

        Ui.Step("2б. Чтение с ошибкой — using всё равно закроет поток");
        try { FileReaderWithUsing.ReadWithException(filePath); }
        catch (InvalidOperationException ex) { Ui.Error($"Перехвачено: {ex.Message}"); }

        Ui.Step("3. Запись и чтение через FileStorage");
        using (var storage = new FileStorage(storagePath))
        {
            storage.Write("Данные записаны через FileStorage.");
            Ui.Line(storage.Read());
            Ui.Line($"Ресурс: {info.Name}");
        }

        Ui.Step("4. Повторный Dispose и работа после него");
        var secondStorage = new FileStorage(storagePath);
        secondStorage.Dispose();
        secondStorage.Dispose();
        Ui.Success("Повторный Dispose прошёл без ошибок.");

        try { secondStorage.Read(); }
        catch (ObjectDisposedException) { Ui.Error("После Dispose читать нельзя — ObjectDisposedException."); }

        Ui.Step("5. Демонстрация GC");
        MemoryDemo.Run();

        Ui.Step("6. Чужой поток: читатель его не закрывает");
        using (var shared = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            Ui.Line(StreamReaderHelper.ReadKeepingOpen(shared));
            Ui.Line($"Поток после чтения открыт: {shared.CanRead}");
        }

        Ui.Step("7. Когда классу нужен IDisposable");
        foreach (var (name, needed, why) in DisposableGuide.Cases)
        {
            if (needed) { Ui.Success($"{name}: нужен — {why}"); }
            else { Ui.Line($"{name}: не нужен — {why}"); }
        }

        Ui.Step("8. Асинхронное освобождение");
        await using (var asyncStorage = new AsyncFileStorage(storagePath))
        {
            Ui.Line("AsyncFileStorage открыт, закроется через DisposeAsync.");
        }

        Ui.Success("Работа программы завершена.");
    }
}
