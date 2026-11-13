using System.Text;
using Variant14.Models;

namespace Variant14.Services;

// Тесты гоняем и сценарии и считаем PASS/FAILасы xe-xe 
public static class ResourceOwnershipTests
{
    public static TestReport RunAll()
    {
        var passed = 0;
        var total = 0;

        void Check(string name, Action body)
        {
            total++;
            try
            {
                body();
                passed++;
                Console.WriteLine($"  PASS  {name}");
            }
            catch (Exception ex) {  Console.WriteLine($"  FAIL  {name}: {ex.Message}"); }
        }

        Check("владелец держит файл один", () =>
        {
            var path = DataPath("ownership-exclusive_ybeyte_ego.bin");
            using (var owner = new FileResourceOwner(path))
            {
                Assert(owner.IsOpen, "поток должен(кому хз) быть открыт");

                AssertThrows<IOException>(() =>
                {
                    using var second = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
                }, "второй дескриптор не должен открываться");
            }

            using var probe = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
            Assert(probe.CanRead, "файл должен освободиться после Dispose");
        });

        Check("leaveOpen=true оставляет поток живым", () =>
        {
            var path = DataPath("leave-open-true_blat.bin");
            var text = "оставили открытым";
            var raw = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None);

            using (var owner = new FileResourceOwner(raw, leaveOpen: true)){ owner.WriteText(text); }

            Assert(raw.CanRead, "поток не должен закрыться");

            raw.Position = 0;
            var buffer = new byte[raw.Length];
            raw.ReadExactly(buffer);
            Assert(Encoding.UTF8.GetString(buffer) == text, "данные не совпали");

            raw.Dispose();
        });

        Check("leaveOpen=false закрывает поток", () =>
        {
            var path = DataPath("leave-me-open-false_nahuy.bin");
            var raw = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            using (var owner = new FileResourceOwner(raw, leaveOpen: false)) {  owner.WriteText("закрыли вместе"); }
            Assert(!raw.CanRead, "поток должен закрыться");

            using var probe = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
            Assert(probe.Length > 0, "файл должен открываться заново");
        });

        return new TestReport(passed, total);
    }

    private static string DataPath(string name) => Path.Combine(AppContext.BaseDirectory, "data", name);

    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }

    private static void AssertThrows<TException>(Action action, string message) where TException : Exception
    {
        try  {  action(); }
        catch (TException) { return; }

        throw new InvalidOperationException(message + ": исключения не было");
    }
}
