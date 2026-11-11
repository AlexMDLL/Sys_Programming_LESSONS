using System.Runtime.InteropServices;

namespace SystemInfoApp.Interop;

public static class WindowsApi
{
    // kernel32.dll — нативная либа Windows, т.ч. тут P/Invoke.
    [DllImport(
        "kernel32.dll",
        EntryPoint = "GetCurrentProcessId",
        CallingConvention = CallingConvention.Winapi)]
    public static extern uint GetCurrentProcessId();
}
