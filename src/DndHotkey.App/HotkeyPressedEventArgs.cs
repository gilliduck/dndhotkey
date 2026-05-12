using DndHotkey.Core;

namespace DndHotkey.App;

internal sealed class HotkeyPressedEventArgs : EventArgs
{
    public HotkeyPressedEventArgs(HotkeyDefinition hotkey)
    {
        Hotkey = hotkey;
    }

    public HotkeyDefinition Hotkey { get; }
}
