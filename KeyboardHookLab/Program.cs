namespace KeyboardHookLab;

// Точка входа. Application.Run поднимает цикл сообщений WinForms — без него ни**я не работает
internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
