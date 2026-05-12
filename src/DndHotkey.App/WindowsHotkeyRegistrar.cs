using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using DndHotkey.Core;
using WinForms = System.Windows.Forms;

namespace DndHotkey.App;

internal sealed class WindowsHotkeyRegistrar : IHotkeyRegistrar
{
    private const int WmHotkey = 0x0312;
    private const uint ModNoRepeat = 0x4000;
    private static int nextId;
    private readonly int id = Interlocked.Increment(ref nextId);
    private HwndSource? source;
    private HotkeyDefinition? hotkey;
    private bool registered;

    public event EventHandler<HotkeyPressedEventArgs>? HotkeyPressed;

    public void Register(HotkeyDefinition hotkey)
    {
        this.hotkey = hotkey;
        source = new HwndSource(new HwndSourceParameters("DndHotkeyHotkeySink")
        {
            Width = 0,
            Height = 0,
            WindowStyle = unchecked((int)0x80000000)
        });
        source.AddHook(WndProc);

        if (!RegisterHotKey(source.Handle, id, ToNativeModifiers(hotkey.Modifiers), ToVirtualKey(hotkey.Key)))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), $"Could not register hotkey '{hotkey}'.");
        }

        registered = true;
    }

    public void Dispose()
    {
        if (registered && source is not null)
        {
            UnregisterHotKey(source.Handle, id);
            registered = false;
        }

        if (source is not null)
        {
            source.RemoveHook(WndProc);
            source.Dispose();
        }
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WmHotkey && wParam.ToInt32() == id && hotkey is not null)
        {
            HotkeyPressed?.Invoke(this, new HotkeyPressedEventArgs(hotkey));
            handled = true;
        }

        return IntPtr.Zero;
    }

    private static uint ToNativeModifiers(HotkeyModifiers modifiers)
    {
        var native = ModNoRepeat;
        if (modifiers.HasFlag(HotkeyModifiers.Alt))
        {
            native |= 0x0001;
        }

        if (modifiers.HasFlag(HotkeyModifiers.Control))
        {
            native |= 0x0002;
        }

        if (modifiers.HasFlag(HotkeyModifiers.Shift))
        {
            native |= 0x0004;
        }

        if (modifiers.HasFlag(HotkeyModifiers.Windows))
        {
            native |= 0x0008;
        }

        return native;
    }

    private static uint ToVirtualKey(string key)
    {
        if (Enum.TryParse<WinForms.Keys>(key, ignoreCase: true, out var parsed))
        {
            return (uint)parsed;
        }

        throw new HotkeyParseException($"'{key}' is not a Windows virtual key.");
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
