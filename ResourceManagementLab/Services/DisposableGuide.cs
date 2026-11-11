namespace ResourceManagementLab.Services;

// Шпаргалка: когда классу реально нужен IDisposable.
public static class DisposableGuide
{
    public static (string Case, bool Needed, string Why)[] Cases =>
    [
        ("Player с Name и Score", false, "одни управляемые поля, закрывать нечего"),
        ("FileStorage с FileStream", true, "внутри файловый дескриптор"),
        ("Report со List<string>", false, "список живёт в управляемой куче"),
        ("SocketManager с Socket", true, "сокет — внешний ресурс"),
        ("Читатель чужого Stream", false, "ресурс не его, закрывать нельзя")
    ];
}
