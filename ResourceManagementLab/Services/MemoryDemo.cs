namespace ResourceManagementLab.Services;

public static class MemoryDemo
{
    public static void Run()
    {
        Ui.Section("=== MEMORY DEMO ===");

        var tiny = new byte[1024];
        var medium = new byte[16 * 1024];
        var big = new byte[1024 * 1024];
        var huge = new byte[1024 * 1024 * 5];

        Ui.Line($"1 KB:  Gen {GC.GetGeneration(tiny)}");
        Ui.Line($"16 KB: Gen {GC.GetGeneration(medium)}");
        Ui.Line($"1 MB:  Gen {GC.GetGeneration(big)}");
        Ui.Line($"5 MB:  Gen {GC.GetGeneration(huge)}");

        Ui.Line($"Сборок Gen 0: {GC.CollectionCount(0)}");
        Ui.Line($"Сборок Gen 1: {GC.CollectionCount(1)}");
        Ui.Line($"Сборок Gen 2: {GC.CollectionCount(2)}");

        // всё что больше ~85 000 байт, съе******(уходит) в Large Object Heap,
        // а LOH считается частью Gen 2 — поэтому большие массивы сразу там.
        // Для обычных объектов решает не размер, а пережили ли они сборку. Так то и так все дохнут, а тут им испытание еще
        tiny = null!;
        medium = null!;
        big = null!;
        huge = null!;

        Ui.Success("Ссылки потеряны — дальше уже дело GC.");

        Ui.Line($"Занято управляемой памяти: {GC.GetTotalMemory(false) / 1024} KB");
    }
}
