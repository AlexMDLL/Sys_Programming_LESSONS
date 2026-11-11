using System.Text;

namespace ResourceManagementLab.Services;

// Класс сам владеет потоком — значит, сам и должен его закрыть.
public sealed class FileStorage : IDisposable
{
    private readonly FileStream _stream;

    // Чтобы не закрыть дважды и не работать после Dispose.
    private bool _disposed;

    public FileStorage(string filePath)
    {
        _stream = new FileStream(
            filePath,
            FileMode.OpenOrCreate,
            FileAccess.ReadWrite,
            FileShare.None);
    }

    public void Write(string text)
    {
        ThrowIfDisposed();

        byte[] data = Encoding.UTF8.GetBytes(text);

        _stream.Write(data);
        _stream.Flush();
    }

    public string Read()
    {
        ThrowIfDisposed();

        _stream.Position = 0;

        // leaveOpen: true — читатель не должен закрывать чужой поток.
        using var reader = new StreamReader(
            _stream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            bufferSize: 1024,
            leaveOpen: true);

        return reader.ReadToEnd();
    }

    public void Dispose()
    {
        // просто ниче не делаем.
        if (_disposed)
        {
            return;
        }

        _stream.Dispose();

        _disposed = true;

        Ui.Success("FileStorage: ресурс освобождён.");
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
