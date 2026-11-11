namespace ResourceManagementLab.Services;

// Нарочно неправильный вариант: поток и читатель создали, а закрыть забыли.
public static class FileReaderWithoutUsing
{
    public static string Read(string filePath)
    {
        var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read);

        var reader = new StreamReader(stream);

        string content = reader.ReadToEnd();

        // Dispose не зовём — так и задумано, это пример говно-кода.
        return content;
    }
}
