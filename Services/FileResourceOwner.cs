namespace Variant14.Services;
public sealed class FileResourceOwner
{
    // Приватное поле — поток нельзя подменить или закрыть извне.
    private readonly FileStream _stream;
    // Открывает сущ или создаёт новый файл и становится хозяином потока
    public FileResourceOwner(string path, FileMode mode = FileMode.OpenOrCreate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        FilePath = Path.GetFullPath(path);

        var directory = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrEmpty(directory)) { Directory.CreateDirectory(directory); }

        // FileShare.None выявляет владельца файла(поток) который не даст другому потоку открыться на этом файле
        _stream = new FileStream(FilePath, mode, FileAccess.ReadWrite, FileShare.None);
    }

    //Полный путь к файлу, которым владеет класс
    public string FilePath { get; }

    //Байтовый размкер файла
    public long Length => _stream.Length;

    //Если поток октрыт читаем / нет идем на**й(Спать)
    public bool IsOpen => _stream.CanRead;
}
