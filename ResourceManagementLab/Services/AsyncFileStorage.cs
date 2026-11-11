namespace ResourceManagementLab.Services;

// Асинхронный вариант: освобождается через await using.
public sealed class AsyncFileStorage : IAsyncDisposable
{
    private readonly FileStream _stream;

    public AsyncFileStorage(string filePath)
    {
        _stream = new FileStream(
            filePath,
            FileMode.OpenOrCreate,
            FileAccess.ReadWrite,
            FileShare.None,
            bufferSize: 4096,
            useAsync: true);
    }

    public async ValueTask DisposeAsync()
    {
        await _stream.DisposeAsync();

        Ui.Success("AsyncFileStorage: ресурс освобождён асинхронно.");
    }
}
