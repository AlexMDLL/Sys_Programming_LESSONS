using System.Net;

namespace ExporterStrategies;

// hw: HTML-отчёт — заголовок в h1, текст в p, дата в footer.
// Текст экранируем, иначе < > & в отчёте сломают разметку.
//
// Про подходы, коротко:
// - адаптер — когда старый класс менять нельзя (чужая либа), просто оборачиваем;
// - наследование — когда класс свой и правда расширяем поведение, но жёстко привязывает;
// - правка старого класса — проще всего, зато ломает совместимость и чужие сборки.
public sealed class HtmlReportExporter : IReportExporter
{
    private readonly string _path;

    public HtmlReportExporter(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _path = path;
    }

    public void Export(ReportDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        string title = WebUtility.HtmlEncode(document.Title);
        string body = WebUtility.HtmlEncode(document.Body)
            .Replace("\r\n", "\n")
            .Replace("\n", "<br />");

        string html =
            "<!DOCTYPE html>\n" +
            "<html lang=\"ru\">\n" +
            "<head><meta charset=\"utf-8\"><title>Отчёт</title></head>\n" +
            "<body>\n" +
            $"<h1>{title}</h1>\n" +
            $"<p>{body}</p>\n" +
            $"<footer>Сформировано: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</footer>\n" +
            "</body>\n" +
            "</html>\n";

        // Ошибки записи не глотаем.
        File.WriteAllText(_path, html);
    }
}
