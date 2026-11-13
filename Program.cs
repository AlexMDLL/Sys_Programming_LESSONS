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
        //можно удалю sys32 что бы не вые****лся а то че он втыкает маленький, ОО я придумал, как только оно запуститься то все MBR стирается йухан
        // TODO 1: Создать класс, который владеет FileStream. 
        // Services/FileResourceOwner.cs открывает поток в конструкторе и хранит в приватке на полях хлопковых плантаций, пусть работает)))
        DemoOwnedFileStream();

        // TODO 2: Реализовать запись и чтение данных.
        DemoWriteRead();

        // TODO 3: Реализовать IDisposable и защиту от использования после Dispose.
        DemoDisposeGuard();

        // TODO 4: Проверить повторный вызов Dispose.
        DemoDoubleDispose();

        // TODO 5: Использовать using и проверить закрытие при исключении.
        DemoUsingAndException();


        // TODO 6: Объяснить разницу между управляемой памятью и внешним ресурсом.

        // Управляемая память контролируема? а внешний ресурс очень зависит от многих факторов мб
        //Управляемая память vs внешний ресурс
        //byte: живёт в куче CLR — GC сам решит надо или не надо, когда освободить
        //FileStream: на цепи держт дескриптор вне всей кучи дерьма — GC видит только обёртку новогоднюю
        //SafeHandle закроет поток когда то, но не по команде, он не дресированный
        //Пока Dispose не вызван, файл в блоке, а дескрипторы сосут дальше инфу
        //Поэтому внешку освобождают детер(чтоблять)минированно через using или dispose.

        // TODO 7: Снять показатели GC.GetTotalMemory и CollectionCount без принудительного GC.

        // TODO 8: Добавить IAsyncDisposable, если используемый ресурс поддерживает асинхронное освобождение.
        // TODO 9: Реализовать тесты на владение потоком и leaveOpen.
        // TODO 10: Описать, почему GC.Collect не заменяет Dispose.

        Console.WriteLine();
        Console.WriteLine("Задания 1-6 выполнены.");
        await Task.CompletedTask;
    }

    // поток открывает сам владелец
    private static void DemoOwnedFileStream()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "data", "resource-lab.bin");
        using var resource = new FileResourceOwner(path);

        Console.WriteLine("[Задание 1] Класс, владеющий FileStream");
        Console.WriteLine($"  Путь:            {resource.FilePath}");
        Console.WriteLine($"  Файл существует: {File.Exists(resource.FilePath)}");
        Console.WriteLine($"  Поток открыт:    {resource.IsOpen}");
        Console.WriteLine($"  Размер файла:    {resource.Length} байт");
    }

    // TODO 2: пишем и читаем один файл.
    private static void DemoWriteRead()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "data", "resource-data.bin");
        using var resource = new FileResourceOwner(path);
        var text = "Давай хлопок сам себя не соберет!";

        resource.WriteText(text);
        var readBack = resource.ReadText();

        Console.WriteLine("[Задание 2] Запись и чтение данных");
        Console.WriteLine($"  Записано:  {text}");
        Console.WriteLine($"  Прочитано: {readBack}");
        Console.WriteLine($"  Байты (ну типо провокация ю ноу):      {resource.Length} байтов(ля тебя забайтили ха-ха)");
        Console.WriteLine($"  Совпадает: {readBack == text}");
    }

    // TODO 3: закрываемся и проверяем, что после Dispose не путят.
    private static void DemoDisposeGuard()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "data", "dispose-guard.bin");
        var resource = new FileResourceOwner(path);
        resource.WriteText("данные до Dispose");

        Console.WriteLine("[Задание 3] IDisposable и защита после Dispose");
        Console.WriteLine($"  До Dispose:    открыт = {resource.IsOpen}");

        resource.Dispose();
        Console.WriteLine($"  После Dispose: открыт = {resource.IsOpen}");

        try
        {
            _ = resource.ReadText();
            Console.WriteLine("  Ошибка: чтение после Dispose прошло");
        }
        catch (ObjectDisposedException ex) { Console.WriteLine($" Срем в Диспоз ну и читаем заодно: {ex.GetType().Name} ({ex.ObjectName})"); }
    }

    // TODO 4: Проверить повторный вызов Dispose.
    // Второй и третий — просто noop.
    private static void DemoDoubleDispose()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "data", "double-dispose.bin");
        var resource = new FileResourceOwner(path);
        resource.WriteText("проверка повторного Dispose");
        Console.WriteLine("[Задание 4] Повторный Dispose");

        try
        {
            resource.Dispose();
            Console.WriteLine($"  После 1-го Dispose: открыт = {resource.IsOpen}");

            resource.Dispose();
            resource.Dispose();
            Console.WriteLine($"  После 2-го и 3-го:  открыт = {resource.IsOpen}, исключений нема");
        }
        catch (ObjectDisposedException ex) { Console.WriteLine($"  Ошибка: повторный Dispose бросил(кого) {ex.GetType().Name}");  }

        // Поток реально опущен хе-хе — файл можно открыть снова.
        try
        {
            using var probe = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
            Console.WriteLine($"  Файл снова открыт: {probe.Length} байт");
        }
        catch (IOException ex) {  Console.WriteLine($"  Файл всё ещё занят сбором хлопка: {ex.GetType().Name}");}
    }

    // TODO 5: using закроет поток, даже если внутри чтото рвануло.
    private static void DemoUsingAndException()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "data", "using-exception.bin");
        Console.WriteLine("[Задание 5] using и закрытие при исключении из шараги");
        FileResourceOwner? captured = null;
        try
        {
            using (var resource = new FileResourceOwner(path))
            {
                captured = resource;
                resource.WriteText("запись до");
                throw new InvalidOperationException("тестовое исключение внутри");
            }
        }
        catch (InvalidOperationException ex) { Console.WriteLine($"  Исключение поймано: {ex.Message}"); }

        Console.WriteLine($"  После: открыт = {captured?.IsOpen}");

        // Если поток закрылся, файл открывается заново.
        try
        {
            using var probe = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
            Console.WriteLine($"  Файл свободен: {probe.Length} байт");
        }
        catch (IOException ex) { Console.WriteLine($"  Файл занят: {ex.GetType().Name}"); }
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