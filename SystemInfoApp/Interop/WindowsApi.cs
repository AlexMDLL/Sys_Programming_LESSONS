using System.Runtime.InteropServices;

namespace SystemInfoApp.Interop;

public static class WindowsApi
{
    // TODO (П1): объявить GetCurrentProcessId из kernel32.dll через DllImport.
    public static uint GetCurrentProcessId()
    {
        throw new NotImplementedException("TODO (П1): DllImport GetCurrentProcessId");
    }
}
