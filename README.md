# Пара 15. Реестр Windows и настройки приложения

Стартовый проект: читаем и пишем настройки в `HKCU\Software\StudentSystemProgramming`.

## Структура

```
StudentSystemProgramming/
├── StudentSystemProgramming.sln
├── StudentSystemProgramming/
│   ├── StudentSystemProgramming.csproj   — net9.0-windows
│   ├── Program.cs                        — запуск: читаем, выбираем тему, пишем
│   ├── Ui.cs                             — цветной вывод
│   ├── Models/AppSettings.cs             — Theme + LaunchCount
│   └── Services/RegistrySettingsService.cs — TODO (П1): Load и Save
└── t/Пара 15. Реестр Windows и настройки приложения.md
```

Запуск:

```
dotnet run --project StudentSystemProgramming
```

Пока `Load` не написан, программа пишет красным «Ещё не реализовано» — так и задумано.

## П1

1. `Load`: открыть ключ через `using`, прочитать `Theme` и `LaunchCount`,
   если ключа нет — вернуть настройки по умолчанию.
2. `Save`: проверить значения и записать их в нашу ветку.
3. Запустить два раза и убедиться, что `LaunchCount` растёт.
4. Сохранить `Dark`, перезапустить — тема должна восстановиться.
5. Посмотреть ветку в `regedit`, удалить `Theme` — должна подставиться `Light`.
6. Удалить всю ветку — приложение должно вести себя как при первом запуске.

## П2

1. Добавить `SchemaVersion` в настройки.
2. `--reset`: удалить только `HKCU\Software\StudentSystemProgramming`.
3. Миграция: если `SchemaVersion` нет, а `Theme`/`LaunchCount` есть — прочитать их
   и записать актуальную версию схемы.
4. `SchemaVersion = 99` — сообщить, что настройки новее приложения.
5. Неизвестная тема (например `Blue`) — использовать `Light`, не падать.

## Контрольные вопросы

1. Чем ключ реестра отличается от значения?
2. Зачем `HKCU`, а не `HKLM`?
3. Почему отсутствие ключа — не ошибка?
4. Зачем `using` для `RegistryKey`?
5. Какие исключения возможны при работе с реестром?
6. Зачем проверять тип прочитанного значения?
7. Что такое версия схемы и чем она отличается от версии сборки?
