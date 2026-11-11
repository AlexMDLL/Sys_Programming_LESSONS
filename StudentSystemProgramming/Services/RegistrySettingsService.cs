using Microsoft.Win32;
using StudentSystemProgramming.Models;

namespace StudentSystemProgramming.Services;

public sealed class RegistrySettingsService
{
    // Наша ветка в HKCU. Чужие не трогаем.
    private const string RegistryPath = @"Software\StudentSystemProgramming";

    // TODO (П1): открыть ключ через using, прочитать Theme и LaunchCount.
    // Если ключа нет — не падаем, отдаём дефолтные настройки.
    public AppSettings Load()
    {
        throw new NotImplementedException("Load: читаем Theme и LaunchCount из реестра");
    }

    // TODO (П1): проверить настройки и записать их в нашу ветку.
    public void Save(AppSettings settings)
    {
        throw new NotImplementedException("Save: пишем Theme и LaunchCount в реестр");
    }
}
