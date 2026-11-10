namespace FileProcessingPipeline.Models;

// Каждое задание содержит идентификатор и путь к файлу.
// Между компонентами передаются самостоятельные объекты заданий,
// а не просто строки.
public sealed record FileTask(
    int Id,
    string FilePath);
