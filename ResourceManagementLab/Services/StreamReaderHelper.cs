using System.Text;

namespace ResourceManagementLab.Services;

// Читает чужой поток и не закрывает его: закрывает тот, кто владеет.
public static class StreamReaderHelper
{
    public static string ReadKeepingOpen(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using var reader = new StreamReader(
            stream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            bufferSize: 1024,
            leaveOpen: true);

        return reader.ReadToEnd();
    }
}
