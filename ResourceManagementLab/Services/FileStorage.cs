namespace ResourceManagementLab.Services;

// Класс сам владеет потоком — значит, сам и должен его закрыть.
public sealed class FileStorage : IDisposable
{
    private readonly FileStream _stream;

    // Чтобы не закрыть дважды и не работать после Dispose.
    private bool _disposed;

    public FileStorage(string filePath)
    {
        // TODO 1 (П2): открой FileStream в режиме OpenOrCreate на чтение и запись.
        throw new NotImplementedException("FileStorage: конструктор");
    }

    public void Write(string text)
    {
        // TODO 2 (П2): проверь, что объект ещё жив.
        // TODO 3 (П2): переведи текст в байты.
        // TODO 4 (П2): запиши байты в поток.
        throw new NotImplementedException("FileStorage.Write");
    }

    public string Read()
    {
        // TODO 5 (П2): проверь состояние объекта.
        // TODO 6 (П2): верни позицию в начало.
        // TODO 7 (П2): прочитай текст, не закрывая чужой поток (leaveOpen).
        throw new NotImplementedException("FileStorage.Read");
    }

    public void Dispose()
    {
        // TODO 8 (П2): не освобождай дважды.
        // TODO 9 (П2): закрой поток.
        // TODO 10 (П2): поставь флаг _disposed.
        throw new NotImplementedException("FileStorage.Dispose");
    }
}
