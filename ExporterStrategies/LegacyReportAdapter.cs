using System.Reflection;

namespace ExporterStrategies;
public sealed class LegacyReportAdapter : IReportExporter
{
    private readonly LegacyReportPrinter _printer;
    public LegacyReportAdapter(LegacyReportPrinter printer) 
    {
        ArgumentNullException.ThrowIfNull(printer);//Тут надо делать проверку на NULL или нет? Если NULL бдует прога же упадет, ай ладно сделаю...
        _printer = printer; 
    }
    public void Export(ReportDocument document)
    {
        // TODO 3: Согласуйте проверку входных данных с контрактом второго экспортёра.
        ArgumentNullException.ThrowIfNull(document);
        _printer.PrintReport(document.Title, document.Body);
    }
}
