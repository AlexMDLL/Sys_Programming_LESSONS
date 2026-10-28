namespace LegacyReportAdapter;
public sealed class LegacyReportAdapter : IReportExporter
{
    private readonly LegacyReportPrinter _printer;
    public LegacyReportAdapter(LegacyReportPrinter printer)
    {
        // TODO 1: Защитите обязательную зависимость от некорректного значения.
        ArgumentNullException.ThrowIfNull(printer);

        _printer = printer;
    }
    public void Export(ReportDocument document)
    {
        // TODO 2: Проверьте входной документ и определите поведение при null.
        ArgumentNullException.ThrowIfNull(document);

        // TODO 3: Преобразуйте единый документ в старый API (заголовок и тело).        //Не нужно этим занимается ReportDocument.cs
        // TODO 4: Не изменяйте LegacyReportPrinter и не дублируйте формат его вывода   //Не нужно этим занимается ReportDocument.cs

        _printer.PrintReport(document.Title, document.Body);
    }
}
