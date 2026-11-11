namespace SystemInfoLibrary;

// Обычный управляемый код, без P/Invoke.
public sealed class SystemInfo
{
    public string GetApplicationName() => "Student System Programming";

    public string GetLibraryVersion() => "1.0.0";

    // Версия рантайма — не то же, что версия библиотеки.
    public string GetRuntimeDescription() => $".NET {Environment.Version}";
}
