using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KeyboardHookLab.Interop;

// Событие из hook: клавиша + признак системной (Alt к примеру).
public readonly record struct HookKeyEvent(Keys Key, bool IsSystemKey);

// Обертываем как подарок на новый год низкоуровневый hook: ставим, ловим, снимаем
public sealed class KeyboardHook : IDisposable
{
    private const int WH_KEYBOARD_LL = 13;

    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104;

    // Windows битой (пи***т)бьет делегат на каждое событие клавиатуры.
    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    // Ссылку держим: иначе эта мразь -> GC может спиздить делегат, пока код на него рассчитывает
    private readonly LowLevelKeyboardProc _callback;

    private IntPtr _hookHandle = IntPtr.Zero;

    public event Action<HookKeyEvent>? KeyDown;

    public bool IsInstalled => _hookHandle != IntPtr.Zero;

    public KeyboardHook() => _callback = HookCallback;

    public void Start()
    {
        // Повторный запуск не должен размножаться хендлами, а то че он, до*** умный что ли, делать ему нечего
        if (IsInstalled)
            return;

        _hookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _callback, IntPtr.Zero, 0);

        if (_hookHandle == IntPtr.Zero)
        {
            int error = Marshal.GetLastWin32Error();
            throw new Win32Exception(error, "Не удалось установить клавиатурный hook");
        }
    }

    public void Stop()
    {
        // Повторный Stop мне неприятно
        if (!IsInstalled)
            return;

        if (!UnhookWindowsHookEx(_hookHandle))
        {
            int error = Marshal.GetLastWin32Error();
            throw new Win32Exception(error, "Не удалось снять клавиатурный hook");
        }

        // Хендл обнуляем только после успешного снятия, иначе проеб***(потеряем)
        _hookHandle = IntPtr.Zero;
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 &&
            (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
        {
            try
            {
                // указатель на нативную структуру (разъе*****) разбираем
                var info = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);

                // Hook системный, поэтому чужие нажатия отсекаем по PIDу актив окна
                if (IsOwnProcessFocused())
                {
                    KeyDown?.Invoke(new HookKeyEvent(
                        (Keys)info.vkCode,
                        wParam == (IntPtr)WM_SYSKEYDOWN));
                }
            }
            catch (Exception ex)
            {
                // Через границу кода исключения не путю
                Debug.WriteLine($"Ошибка в callback hookе: {ex}");
            }
        }

        // Событие не блокируем, отдаём дальше по цепочке
        return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }

    private static bool IsOwnProcessFocused()
    {
        IntPtr foreground = GetForegroundWindow();

        if (foreground == IntPtr.Zero)
            return false;

        GetWindowThreadProcessId(foreground, out uint processId);

        return processId == (uint)Environment.ProcessId;
    }

    public void Dispose()
    {
        // Не суем исключение в  Dispose(), лог ведем
        try { Stop(); }
        catch (Win32Exception ex) { Debug.WriteLine($"Hook не снялся: {ex}"); }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KBDLLHOOKSTRUCT
    {
        public uint vkCode;
        public uint scanCode;
        public uint flags;
        public uint time;
        public UIntPtr dwExtraInfo;
    }

    [DllImport("user32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
    private static extern IntPtr SetWindowsHookEx(
        int idHook,
        LowLevelKeyboardProc callback,
        IntPtr hMod,
        uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CallingConvention = CallingConvention.Winapi)]
    private static extern IntPtr CallNextHookEx(
        IntPtr hhk,
        int nCode,
        IntPtr wParam,
        IntPtr lParam);

    [DllImport("user32.dll", CallingConvention = CallingConvention.Winapi)]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", CallingConvention = CallingConvention.Winapi)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
}
