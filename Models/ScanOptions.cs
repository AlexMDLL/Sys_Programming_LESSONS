namespace AsyncFileScanner.Models;

// Модель содержит настройки сканирования.
public sealed record ScanOptions(
    string RootPath,
    IReadOnlySet<string> Extensions,
    long MinBytes = 0,                 //меньше этого размера файл не берём
    long MaxBytes = long.MaxValue,     //больше этого тоже
    int MaxConcurrency = 4);           //сколько файлов хаваем одновременно
