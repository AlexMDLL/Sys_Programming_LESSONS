# Пара 14. Память, GC и освобождение ресурсов

Стартовый проект: управляемая память, поколения GC, `IDisposable` и `using`.

## Структура

```
ResourceManagementLab/
├── ResourceManagementLab.sln
├── ResourceManagementLab/
│   ├── Program.cs             — сценарии лабы, цветной вывод
│   ├── Ui.cs                  — помощник для цветного вывода
│   ├── Models/ResourceInfo.cs
│   ├── Services/
│   │   ├── FileReaderWithoutUsing.cs  — нарочно плохой пример
│   │   ├── FileReaderWithUsing.cs     — TODO (П1): чтение через using
│   │   ├── MemoryDemo.cs              — TODO (П1): поколения и счётчики GC
│   │   └── FileStorage.cs             — TODO (П2): свой IDisposable
│   └── Data/sample.txt
└── t/Пара 14. Память, GC и освобождение ресурсов.md
```

Запуск:

```
dotnet run --project ResourceManagementLab
```

Пока П2 не сделан, шаги 3–4 печатают «Ещё не реализовано» — так и задумано.

## П1

1. Найти в `FileReaderWithoutUsing` объекты, которые требуют освобождения, и объяснить,
   кто владеет `FileStream`.
2. Переписать чтение в `FileReaderWithUsing` через `using`.
3. Добавить `ReadWithException`: прочитать файл и упасть исключением после чтения —
   проверить, что `using` всё равно закрывает поток.
4. Проверить поколения GC: три массива, `GC.GetGeneration`, `GC.CollectionCount(0/1/2)`,
   объяснить, почему размер не определяет поколение.

## П2

5. Реализовать `FileStorage`: конструктор, `Write`, `Read`, `Dispose` (TODO 1–10).
6. Защита от использования после `Dispose` — `ObjectDisposedException`.
7. Повторный `Dispose` не должен падать.
8. Владение ресурсом: `leaveOpen: true`, чтобы читатель не закрывал чужой поток.
9. Определить, когда реально нужен `IDisposable` (таблица из методички, ответы — в комментариях).
10. Дополнительно: `AsyncFileStorage` с `IAsyncDisposable`.

## Контрольные вопросы

1. Чем стек отличается от управляемой кучи?
2. Что такое достижимость объекта?
3. Зачем GC поколения?
4. Почему GC не закрывает файл сразу после потери ссылки?
5. Что такое `IDisposable`?
6. Как работает `using`?
7. Почему `Dispose()` вызывается и при исключении?
8. Чем `Dispose()` отличается от финализатора?
9. Когда нужен `IAsyncDisposable`?
10. Почему не стоит вызывать `GC.Collect()` вручную?
11. Что значит «владеть ресурсом»?
12. Зачем `leaveOpen`?
