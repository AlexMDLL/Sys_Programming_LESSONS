namespace ResourceManagementLab.Models;

// Короткая карточка ресурса: че за файл и освобождён ли он.
public sealed record ResourceInfo(
    string Name,
    bool IsDisposed);
