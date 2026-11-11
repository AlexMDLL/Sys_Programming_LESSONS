using ExporterStrategies;//КАКОЙ NAMESPACE, Я ДУМАЛ ЧТО КОД ВЕСЬ МЕНЯТЬ НАДО, ЗА ЧТО ТАК ПУГАТЬ ТО
var mode = args.Length > 0 ? args[0] : "console";
// Экспортёр выбираем тут, ReportService про это не знает.
IReportExporter exporter = mode.ToLowerInvariant() switch
{
    "file" => new TextFileReportExporter(Path.Combine(Path.GetTempPath(), "daily-summary.txt")),
    "html" => new HtmlReportExporter(Path.Combine(Path.GetTempPath(), "daily-summary.html")),
    _ => new LegacyReportAdapter(new LegacyReportPrinter())
};
var service = new ReportService(exporter);
service.PublishDailySummary(totalJobs: 42, failedJobs: 2);
Console.WriteLine($"Использован экспортёр: {exporter.GetType().Name}");
