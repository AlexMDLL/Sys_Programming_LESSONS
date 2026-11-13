using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Channels;
using Variant14.Services;

namespace Variant14;

internal static class Program
{
    private static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("Зачётный проект 14: Менеджер ресурсов");
        Console.WriteLine(new string('=', 64));

        // Стартовый сценарий нужен для проверки структуры проекта. // оке
        // Реализацию индивидуальных требований выполняйте в ProjectService.
        var service = new ProjectService();
        await service.RunAsync();

        Console.WriteLine();
        Console.WriteLine("Нажмите Enter для завершения.");
        Console.ReadLine();
    }
}

public sealed class ProjectService 
{
    public async Task RunAsync()
    {
        // TODO: перенесите предметную логику в отдельные методы и классы.
        // TODO: не размещайте весь проект в Main.
        // TODO 1: Создать класс, который владеет FileStream. 
        // Services/FileResourceOwner.cs открывает поток в конструкторе и хранит в приватке на полях хлопковых плантаций, пусть работает)))
        DemoOwnedFileStream();

        // TODO 2: Реализовать запись и чтение данных.
        DemoWriteRead();

        // TODO 3: Реализовать IDisposable и защиту от использования после Dispose.
        // TODO 4: Проверить повторный вызов Dispose.
        // TODO 5: Использовать using и проверить закрытие при исключении.
        // TODO 6: Объяснить разницу между управляемой памятью и внешним ресурсом.
        // TODO 7: Снять показатели GC.GetTotalMemory и CollectionCount без принудительного GC.

        // TODO 8: Добавить IAsyncDisposable, если используемый ресурс поддерживает асинхронное освобождение.
        // TODO 9: Реализовать тесты на владение потоком и leaveOpen.
        // TODO 10: Описать, почему GC.Collect не заменяет Dispose.

        Console.WriteLine();
        Console.WriteLine("Задания 1-2 выполнены.");
        await Task.CompletedTask;
    }

    // поток открывает сам владелец
    private static void DemoOwnedFileStream()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "data", "resource-lab.bin");
        var resource = new FileResourceOwner(path);

        Console.WriteLine("[Задание 1] Класс, владеющий FileStream");
        Console.WriteLine($"  Путь:            {resource.FilePath}");
        Console.WriteLine($"  Файл существует: {File.Exists(resource.FilePath)}");
        Console.WriteLine($"  Поток открыт:    {resource.IsOpen}");
        Console.WriteLine($"  Размер файла:    {resource.Length} байт");

        // Освобождение потока добавим в TODO 3 (IDisposable).
    }

    // TODO 2: пишем и читаем один файл.
    private static void DemoWriteRead()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "data", "resource-data.bin");
        var resource = new FileResourceOwner(path);
        var text = "Давай хлопок сам себя не соберет!";

        resource.WriteText(text);
        var readBack = resource.ReadText();

        Console.WriteLine("[Задание 2] Запись и чтение данных");
        Console.WriteLine($"  Записано:  {text}");
        Console.WriteLine($"  Прочитано: {readBack}");
        Console.WriteLine($"  Байты (ну типо провокация ю ноу):      {resource.Length}");
        Console.WriteLine($"  Совпадает: {readBack == text}");
    }
}
/*
git init
git config --global user.email "foxifoxovich394@gmail.com"
git config --global user.name "AlexMDLL"
git remote remove origin
git remote add origin https://github.com/AlexMDLL/Sys_Programming_LESSONS.git
git checkout ZACHERT
git branch -M ZACHERT
git add .
git commit -m "ZACHERT_Skelet"
git push -u origin ZACHERT --force
 */