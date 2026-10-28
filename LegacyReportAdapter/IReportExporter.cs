namespace LegacyReportAdapter;
public interface IReportExporter
{
    // Современный контракт: один объект документа вместо двух строк.
    void Export(ReportDocument document);
}
