using SystemInfoApp;
using SystemInfoApp.Interop;
using SystemInfoLibrary;

try
{
    Ui.Section("Информация о приложении");
    Ui.Line("-----------------------");

    // Инфа из управляемой DLL.
    var systemInfo = new SystemInfo();

    Ui.Line($"Название: {systemInfo.GetApplicationName()}");
    Ui.Line($"Версия библиотеки: {systemInfo.GetLibraryVersion()}");

    // PID уже из нативной kernel32.dll.
    uint processId = WindowsApi.GetCurrentProcessId();
    Ui.Line($"PID процесса: {processId}");
}
catch (NotImplementedException ex)
{
    Ui.Error($"Ещё не готово: {ex.Message}");
}

Ui.Line();
Ui.Step("Enter — выход.");
Console.ReadLine();
