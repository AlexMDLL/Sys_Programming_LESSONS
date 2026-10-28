using LegacyReportAdapter;
using Adapter = LegacyReportAdapter.LegacyReportAdapter;
var failed = new List<string>();
int passed = 0;
void Check(string name, bool condition)
{
    var previous = Console.ForegroundColor;
    try
    {
        Console.ForegroundColor = condition ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine($"{(condition ? "PASS" : "FAIL")} — {name}");
    }
    finally { Console.ForegroundColor = previous; }

    if (condition) passed++;
    else failed.Add(name);
}
var output = new StringWriter();
var original = Console.Out;
IReportExporter exporter = new Adapter(new LegacyReportPrinter());//Ну чтобы всем хватило и не пришлось пересоздавать
try
{
    Console.SetOut(output);
    //Забрал от сюда exporter
    exporter.Export(new ReportDocument("Test title", "Test body"));
}
finally { Console.SetOut(original); }
var text = output.ToString();
Check("заголовок передан legacy-принтеру", text.Contains("=== Test title ==="));
Check("тело передано legacy-принтеру", text.Contains("Test body"));
Check("сохранён завершающий маркер", text.Contains("=== END OF REPORT ==="));
// TODO 5: Добавьте тест пустого заголовка или тела согласно контракту.

//Заголовок
var PustoV_TiTle = new StringWriter();
try
{
    Console.SetOut(PustoV_TiTle);
    exporter.Export(new ReportDocument("", "Test body"));
}
finally { Console.SetOut(original); }
var PustoV_TiTle_TXT = PustoV_TiTle.ToString().Replace("\r\n", "\n");
Check("пустой заголовок не ломает формат", PustoV_TiTle_TXT.Contains("===  ==="));
Check("при пустом заголовке тело на месте", PustoV_TiTle_TXT.Contains("Test body"));

//Тело
var PustoV_BOdy = new StringWriter();
try
{
    Console.SetOut(PustoV_BOdy);
    exporter.Export(new ReportDocument("Only title", ""));
}
finally { Console.SetOut(original); }
var PustoV_BOdy_TXT = PustoV_BOdy.ToString().Replace("\r\n", "\n");
Check("пустое тело сохраняет шапку и подвал", PustoV_BOdy_TXT.Contains("=== Only title ===\n\n=== END OF REPORT ===\n"));

// TODO 6: Добавьте тест null-документа (можно использовать null!).
var DocIsNULL = false;
try { exporter.Export(null!); }
catch (ArgumentNullException) { DocIsNULL = true; }
Check("null-документ отклоняется", DocIsNULL);//ха-ха 67, ладно шучу

// TODO 7: Добавьте тест, что конструктор не принимает null-зависимость.
var Cheta_Printer_Null = false;
try { _ = new Adapter(null!); }
catch (ArgumentNullException) { Cheta_Printer_Null = true; }
Check("конструктор отклоняет null-зависимость", Cheta_Printer_Null);

Console.WriteLine($"Провалено базовых проверок: {failed.Count} из 7.");
