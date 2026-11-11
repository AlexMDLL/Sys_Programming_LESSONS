using StudentSystemProgramming;
using StudentSystemProgramming.Models;
using StudentSystemProgramming.Services;

var service = new RegistrySettingsService();

try
{
    // Отдельный режим: только сброс своей ветки.
    if (args.Length > 0 && args[0] == "--reset")
    {
        service.Reset();
        Ui.Success("Учебные настройки сброшены.");
        return;
    }

    AppSettings settings = service.Load();

    Ui.Section("Настройки приложения");
    Ui.Line("--------------------");
    Ui.Line($"Тема: {settings.Theme}");
    Ui.Line($"Запусков: {settings.LaunchCount}");
    Ui.Line($"Версия схемы: {settings.SchemaVersion}");

    Ui.Line();
    Ui.Step("Выберите тему:");
    Ui.Line("1 - Light");
    Ui.Line("2 - Dark");
    Ui.Line("Enter - оставить текущую");

    string? choice = Console.ReadLine();

    if (choice == "1")
    {
        settings.Theme = "Light";
    }
    else if (choice == "2")
    {
        settings.Theme = "Dark";
    }

    // checked — чтобы счётчик не переполнился молча.
    settings.LaunchCount = checked(settings.LaunchCount + 1);

    service.Save(settings);

    Ui.Line();
    Ui.Success("Настройки сохранены.");
    Ui.Success($"Тема: {settings.Theme}");
    Ui.Success($"Запуск № {settings.LaunchCount}");
}
catch (UnauthorizedAccessException ex)
{
    Ui.Error($"Нет доступа к реестру: {ex.Message}");
}
// InvalidDataException наследник IOException, поэтому ловим его первым.
catch (InvalidDataException ex)
{
    Ui.Error($"Некорректные настройки: {ex.Message}");
}
catch (IOException ex)
{
    Ui.Error($"Ошибка работы с реестром: {ex.Message}");
}
catch (OverflowException ex)
{
    Ui.Error($"Счётчик запусков переполнен: {ex.Message}");
}
catch (ArgumentException ex)
{
    Ui.Error($"Некорректные данные: {ex.Message}");
}
