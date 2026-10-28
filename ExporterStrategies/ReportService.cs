namespace ExporterStrategies;
public sealed class ReportService
{
    private readonly IReportExporter _exporter;
    public ReportService(IReportExporter exporter)
    {
        // TODO 1: Проверьте обязательную зависимость.
        ArgumentNullException.ThrowIfNull(exporter);
        _exporter = exporter;
    }
    public void PublishDailySummary(int totalJobs, int failedJobs)
    {
        // TODO 2: Бизнес-логика формирует документ, но не знает способ экспорта. //А смысл? Если документ формируется тут, зачем ему знать форму експорта?
        var body = $"Всего задач: {totalJobs}\nОшибок: {failedJobs}";
        _exporter.Export(new ReportDocument("Сводка за день", body));
    }
}
