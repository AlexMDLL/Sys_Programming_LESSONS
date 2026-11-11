using StudentSystemProgramming;
using StudentSystemProgramming.Models;
using StudentSystemProgramming.Services;

var service = new RegistrySettingsService();

try
{
    AppSettings settings = service.Load();

    Ui.Section("Настройки приложения");
    Ui.Line("--------------------");

    Ui.Line($"Тема: {settings.Theme}");
    Ui.Line($"Запусков: {settings.LaunchCount}");

    Ui.Line();
    Ui.Step("Выберите тему:");
    Ui.Line("1 - Light");
    Ui.Line("2 - Dark");

    string? choice = Console.ReadLine();

    if (choice == "1")
    {
        settings.Theme = "Light";
    }
    else if (choice == "2")
    {
        settings.Theme = "Dark";
    }

    settings.LaunchCount++;

    service.Save(settings);

    Ui.Line();
    Ui.Success("Настройки сохранены.");
    Ui.Success($"Запуск № {settings.LaunchCount}");
}
catch (NotImplementedException ex)
{
    Ui.Error($"Ещё не реализовано: {ex.Message}");
}
catch (UnauthorizedAccessException ex)
{
    Ui.Error($"Нет доступа к реестру: {ex.Message}");
}
catch (IOException ex)
{
    Ui.Error($"Ошибка работы с реестром: {ex.Message}");
}
catch (Exception ex)
{
    Ui.Error($"Непредвиденная ошибка: {ex.Message}");
}
