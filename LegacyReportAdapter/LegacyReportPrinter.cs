namespace LegacyReportAdapter;

// Имитирует внешнюю библиотеку. По условию занятия этот класс изменять нельзя.
public sealed class LegacyReportPrinter
{
    public void PrintReport(string reportTitle, string reportText)
    {
        Console.WriteLine($"=== {reportTitle} ===");
        Console.WriteLine(reportText);
        Console.WriteLine("=== END OF REPORT ===");
    }
}
