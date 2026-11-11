namespace ResourceManagementLab;

// Сделал лишнюю, а то красить ручками не хочу, а так будет поудобнее когда что вызывается то и красится.
public static class Ui
{
    public static void Line(string text = "", ConsoleColor color = ConsoleColor.Gray)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    public static void Section(string text) => Line(text, ConsoleColor.Cyan);

    public static void Step(string text) => Line(text, ConsoleColor.White);

    public static void Success(string text) => Line(text, ConsoleColor.Green);

    public static void Error(string text) => Line(text, ConsoleColor.Red);
}
