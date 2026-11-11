using SystemInfoLibrary;
using SystemInfoSecondApp;

var systemInfo = new SystemInfo();

Ui.Section("Второе приложение");
Ui.Line("------------------");

Ui.Line($"Название: {systemInfo.GetApplicationName()}");
Ui.Line($"Версия библиотеки: {systemInfo.GetLibraryVersion()}");
Ui.Line($"Среда выполнения: {systemInfo.GetRuntimeDescription()}");
