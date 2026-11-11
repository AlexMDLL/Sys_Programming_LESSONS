namespace ResourceManagementLab.Services;

public static class FileReaderWithUsing
{
    // using сам вызовет Dispose — и на обычном выходе, и если что-то упало.
    public static string Read(string filePath)
    {
        using var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read);

        // StreamReader в стоке закрывает переданный поток,
        // не ссы: повторный Dispose у FileStream ниче не ломает.
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }

    // Читаем нормально, а падаем уже после чтения — поток всё равно закроется.
    public static string ReadWithException(string filePath)
    {
        using var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read);

        using var reader = new StreamReader(stream);

        // Просто заставляем поток реально поработать.
        _ = reader.ReadToEnd();

        throw new InvalidOperationException(
            "Искусственная ошибка после чтения файла.");
    }
}
