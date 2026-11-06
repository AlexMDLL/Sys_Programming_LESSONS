# Пара 12. PLINQ и параллельная обработка коллекций

Стартовый проект: анализ журнала игровых событий двумя способами — LINQ и PLINQ.

## Структура

```
GameEventsAnalyzer/
├── GameEventsAnalyzer.sln
├── GameEventsAnalyzer/
│   ├── Program.cs            — контрольные сценарии, лента событий, замеры
│   ├── GameEvent.cs          — модель события
│   ├── PlayerStatistics.cs   — модель статистики игрока
│   ├── GameEventGenerator.cs — генератор событий с фиксированным seed
│   ├── GameEventAnalyzer.cs  — TODO: анализ LINQ/PLINQ, лента событий
│   └── BenchmarkRunner.cs    — замеры LINQ и PLINQ
├── GameEventsAnalyzer.Tests/
│   └── GameEventAnalyzerTests.cs — xUnit-тесты
└── t/
    └── Пара 12. PLINQ и параллельная обработка коллекций.md
```

Запуск:

```
dotnet run --project GameEventsAnalyzer            # всё
dotnet run --project GameEventsAnalyzer -- checks  # контрольные сценарии П1
dotnet run --project GameEventsAnalyzer -- order   # лента событий П2
dotnet run --project GameEventsAnalyzer -- bench   # замеры LINQ/PLINQ
dotnet test
```

## П1. Аналитика через LINQ и PLINQ

Задача: отобрать события `Kill`, сгруппировать по `PlayerId`, посчитать
количество убийств и сумму очков, отсортировать по очкам.

Контрольный сценарий:

| Игрок | Убийств | Очков |
| ----- | ------- | ----- |
| 1     | 2       | 250   |
| 2     | 1       | 200   |

Дополнительно: пустая коллекция; коллекция без `Kill`; все убийства одного
игрока; `null` → `ArgumentNullException`.

Задания:

1. Реализовать `AnalyzeWithLinq`.
2. Реализовать `AnalyzeWithPlinq` на `AsParallel()`.
3. Сортировка по `TotalPoints` по убыванию.
4. `ThenBy` по `PlayerId` для стабильного порядка.
5. Сравнить результаты через `SequenceEqual`.
6. Проверить пустую коллекцию.
7. Проверить коллекцию без `Kill`.
8. Проверить коллекцию с одним игроком.
9. Проверить `null`.
10. Объяснить, почему PLINQ не обязательно быстрее LINQ.

## П2. Порядок и `AsOrdered`

Задача: лента событий `Kill` в исходном порядке.

Задания:

1. `GetKillsUnordered` — без `AsOrdered`.
2. `GetKillsOrdered` — с `AsOrdered`.
3. Проверить сохранение исходного порядка.
4. `GetKillsSortedByTime` — сортировка по `Timestamp`.
5. `ThenBy` по `Id`.
6. Убрать `AsOrdered` из запроса с `OrderBy` и проверить порядок.
7. Изменить генератор так, чтобы встречались одинаковые `Timestamp`.
8. Объяснить, зачем `ThenBy` по `Id`.
9. Сравнить время вариантов с `AsOrdered` и без него.
10. Объяснить, почему сохранение порядка снижает производительность.

## Вопросы

1. Что делает `AsParallel()`?
2. Чем `ParallelQuery<T>` отличается от `IEnumerable<T>`?
3. Зачем `Where` и `Select` в PLINQ?
4. Что делает `Aggregate`?
5. Почему PLINQ по умолчанию не сохраняет порядок?
6. Зачем нужен `AsOrdered()`?
7. Чем `AsOrdered()` отличается от `OrderBy()`?
8. Почему нельзя изменять общий `List<T>` из `ForAll`?
9. Когда PLINQ может быть медленнее LINQ?
10. Когда лучше обычный LINQ?

## Домашнее задание

Добавить статистику по типу события (количество и сумма очков) двумя способами —
LINQ и PLINQ, сравнить результаты, провести по пять измерений для размеров
1 000, 100 000 и 1 000 000 событий и написать вывод о производительности.
