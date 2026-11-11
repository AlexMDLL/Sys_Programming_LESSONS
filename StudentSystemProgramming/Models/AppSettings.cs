namespace StudentSystemProgramming.Models;

// Тут только данные, про реестр ни слова.
public sealed class AppSettings
{
    public string Theme { get; set; } = "Light";

    public int LaunchCount { get; set; }

    // Версия формата настроек, а не версия сборки.
    public int SchemaVersion { get; set; } = 1;
}
