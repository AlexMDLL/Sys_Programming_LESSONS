# Пара 16. DLL, сборки .NET и P/Invoke

Стартовый проект: своя библиотека (`SystemInfoLibrary.dll`) + вызов `kernel32.dll` через P/Invoke.

## Структура

```
DllCourse/
├── DllCourse.sln
├── SystemInfoLibrary/          — classlib, net9.0
│   └── SystemInfo.cs           — методы библиотеки
├── SystemInfoApp/              — console, net9.0-windows
│   ├── Program.cs              — вывод
│   ├── Ui.cs                   — цветной вывод
│   └── Interop/WindowsApi.cs   — P/Invoke kernel32.dll
├── SystemInfoSecondApp/        — console, net9.0-windows, та же DLL
│   ├── Program.cs
│   └── Ui.cs
└── t/Пара 16. DLL, сборки .NET и P.Invoke.md
```

Запуск:

```
dotnet build
dotnet run --project SystemInfoApp
dotnet run --project SystemInfoSecondApp
```

## П1

1. `SystemInfo.GetApplicationName()` и `GetLibraryVersion()` — обычный управляемый код.
2. `WindowsApi.GetCurrentProcessId()` — `[DllImport]` из `kernel32.dll`, `uint` под `DWORD`.
3. Запустить, увидеть название, версию и PID.
4. Попробовать сломать `EntryPoint` → `EntryPointNotFoundException`, вернуть обратно.
5. Убрать `Project Reference` → ошибка компиляции, вернуть.

## П2

1. Добавить в библиотеку `GetRuntimeDescription()` (`Environment.Version`).
2. Создать второй проект `SystemInfoSecondApp` и подключить ту же DLL.
3. Убедиться, что обе программы используют одну библиотеку.
4. Проверить, что `SystemInfoLibrary.dll` копируется в оба `bin`.

## Контрольные вопросы

1. Чем управляемая сборка отличается от нативной DLL?
2. Зачем `Project Reference`, а не копирование файла?
3. Что делает `[DllImport]` и зачем `EntryPoint`?
4. Почему `DWORD` → `uint`, а не `int`?
5. Когда `DllNotFoundException`, а когда `EntryPointNotFoundException`?
6. Почему `BadImageFormatException` связан с разрядностью?
7. Почему библиотека работает на Linux, а P/Invoke на `kernel32.dll` — нет?
