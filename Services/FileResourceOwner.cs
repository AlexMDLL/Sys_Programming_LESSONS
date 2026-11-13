using System.Text;

namespace Variant14.Services;

// Хозяин FileStream: сам открыл — сам закрыл
public sealed class FileResourceOwner : IDisposable
{
    private readonly FileStream _stream; // живёт только тут, наружу не отдаём
    private bool _disposed; // уже закрылись?

    public FileResourceOwner(string path, FileMode mode = FileMode.OpenOrCreate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        FilePath = Path.GetFullPath(path);

        var directory = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        _stream = new FileStream(FilePath, mode, FileAccess.ReadWrite, FileShare.None);
    }

    public string FilePath { get; }

    public long Length
    {
        get
        {
            ThrowIfDisposed();
            return _stream.Length;
        }
    }

    public bool IsOpen => !_disposed && _stream.CanRead;

    // Пишет с начала, старый хвост обрезает
    public void Write(ReadOnlySpan<byte> data)
    {
        ThrowIfDisposed();
        _stream.Position = 0;
        _stream.Write(data);
        _stream.SetLength(data.Length);
        _stream.Flush();
    }

    // Читает файл целиком с начала
    public byte[] ReadAll()
    {
        ThrowIfDisposed();
        _stream.Position = 0;
        var buffer = new byte[_stream.Length];
        _stream.ReadExactly(buffer);
        return buffer;
    }

    public void WriteText(string text) => Write(Encoding.UTF8.GetBytes(text));

    public string ReadText() => Encoding.UTF8.GetString(ReadAll());

    // Закрываемся один раз; повторный вызов — не ошибка а так задумано, ну в принципе в 4 туду реализация будэ
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _stream.Dispose();
    }

    // Поток закрыт — дальше низя, дальше бога нет ну в принципе как в кабинете физики или че там
    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
