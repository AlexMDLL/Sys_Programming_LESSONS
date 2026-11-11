using ExporterStrategies;
using Microsoft.VisualBasic;
var failed = new List<string>();
int passed = 0;
void Check(string name, bool condition)
{
    var previous = Console.ForegroundColor;
    try
    {
        Console.ForegroundColor = condition ? ConsoleColor.Green : ConsoleColor.Red;    // Я просто хочу красиво, простите
        Console.WriteLine($"{(condition ? "PASS" : "FAIL")} — {name}");
    }
    finally { Console.ForegroundColor = previous; }

    if (condition) passed++;
    else failed.Add(name);
}
var path = Path.Combine(Path.GetTempPath(), $"report-test-{Guid.NewGuid():N}.txt");
try
{
    IReportExporter fileExporter = new TextFileReportExporter(path);
    var service = new ReportService(fileExporter);
    service.PublishDailySummary(10, 1);
    var contents = File.ReadAllText(path);
    Check("файловый экспорт создаёт файл", File.Exists(path));
    Check("файл содержит заголовок", contents.Contains("Сводка за день"));
    Check("файл содержит значения отчёта", contents.Contains("Всего задач: 10") && contents.Contains("Ошибок: 1"));

// TODO 7: Добавьте проверку LegacyReportAdapter через Console.Out.
//            +
// TODO 8: Докажите, что ReportService не меняется при переключении экспортёра.

var Console_Vihod = new StringWriter();
var original = Console.Out;
try
{
    Console.SetOut(Console_Vihod);
    var legacyService = new ReportService(new LegacyReportAdapter(new LegacyReportPrinter()));
    legacyService.PublishDailySummary(10, 1);
}
finally { Console.SetOut(original); }
var Consol_TXT = Console_Vihod.ToString().Replace("\r\n", "\n");

Check("legacy: заголовок в прежнем формате", Consol_TXT.Contains("=== Сводка за день ==="));
Check("legacy: тело отчёта", Consol_TXT.Contains("Всего задач: 10") && Consol_TXT.Contains("Ошибок: 1"));
Check("legacy: завершающий маркер", Consol_TXT.Contains("=== END OF REPORT ==="));
Check("ReportService одинаков для обоих экспортёров",
    contents.Contains("Сводка за день") && contents.Contains("Всего задач: 10")
    && Consol_TXT.Contains("Сводка за день") && Consol_TXT.Contains("Всего задач: 10"));
}
finally { if (File.Exists(path)) File.Delete(path); }

// TODO 9: Проверьте контракт пустого документа.

var emptyPath = Path.Combine(Path.GetTempPath(), $"report-empty-{Guid.NewGuid():N}.txt");
try
{
    new TextFileReportExporter(emptyPath).Export(new ReportDocument("", ""));
    Check("пустой документ не ломает файловый экспорт", File.Exists(emptyPath));
    Check("пустой документ даёт пустое содержимое", File.ReadAllText(emptyPath).Trim().Length == 0);
}
finally { if (File.Exists(emptyPath)) File.Delete(emptyPath); }

var pusto_V_consoli = new StringWriter();
var pusto = Console.Out;
try
{
    Console.SetOut(pusto_V_consoli);
    new LegacyReportAdapter(new LegacyReportPrinter()).Export(new ReportDocument("", ""));
}
finally { Console.SetOut(pusto); }
Check("пустой документ даёт ===  === у legacy", pusto_V_consoli.ToString().Contains("===  ==="));


// TODO 10: Проверьте ошибочный путь и отсутствие подавления ошибки записи.

var badPath = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}", "report.txt");
var ErrorPropal = false;
try { new TextFileReportExporter(badPath).Export(new ReportDocument("T", "B")); }
catch (DirectoryNotFoundException) { ErrorPropal = true; }
Check("ошибка записи не подавляется", ErrorPropal);
Check("при ошибке записи файл не создан", !File.Exists(badPath));

var BadPath_Otcazano = false;
try { _ = new TextFileReportExporter("   "); }
catch (ArgumentException) { BadPath_Otcazano = true; }
Check("пустой путь отклоняется", BadPath_Otcazano);

var DocNULL_Otcazano = false;
try { new TextFileReportExporter(path).Export(null!); }
catch (ArgumentNullException) { DocNULL_Otcazano = true; }
Check("null-документ отклоняется", DocNULL_Otcazano);

var ExportNULL_Propal = false;
try { _ = new ReportService(null!); }
catch (ArgumentNullException) { ExportNULL_Propal = true; }
Check("ReportService отклоняет null-экспортёр", ExportNULL_Propal);


// hw: HTML-экспортёр — h1, p, footer + экранирование.
var htmlPath = Path.Combine(Path.GetTempPath(), $"report-{Guid.NewGuid():N}.html");
try
{
    new HtmlReportExporter(htmlPath).Export(
        new ReportDocument("Тест <b>заголовок</b>", "Тело & <script>alert(1)</script>"));

    var html = File.ReadAllText(htmlPath);

    Check("html: файл создан", File.Exists(htmlPath));
    Check("html: заголовок в h1", html.Contains("<h1>Тест &lt;b&gt;заголовок&lt;/b&gt;</h1>"));
    Check("html: текст в p", html.Contains("<p>") && html.Contains("&amp;"));
    Check("html: дата в footer", html.Contains("<footer>Сформировано:"));
    Check("html: разметка из отчёта экранирована", !html.Contains("<script>"));
}
finally { if (File.Exists(htmlPath)) File.Delete(htmlPath); }

var HtmlNULL_Otcazano = false;
try { new HtmlReportExporter(htmlPath).Export(null!); }
catch (ArgumentNullException) { HtmlNULL_Otcazano = true; }
Check("html: null-документ отклоняется", HtmlNULL_Otcazano);

var Html_Oshibka = false;
try
{
    new HtmlReportExporter(
        Path.Combine(Path.GetTempPath(), $"нет-{Guid.NewGuid():N}", "report.html"))
        .Export(new ReportDocument("T", "B"));
}
catch (DirectoryNotFoundException) { Html_Oshibka = true; }
Check("html: ошибка записи не подавляется", Html_Oshibka);


Console.WriteLine($"Провалено базовых проверок: {failed.Count} из {failed.Count + passed}.");
