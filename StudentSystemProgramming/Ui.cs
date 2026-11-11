namespace StudentSystemProgramming;

// Красить каждую строку руками лень, пусть будет один помощник.
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
