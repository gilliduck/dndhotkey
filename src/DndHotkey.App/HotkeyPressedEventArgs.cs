using DndHotkey.Core;

namespace DndHotkey.App;

internal sealed class HotkeyPressedEventArgs : EventArgs
{
    public HotkeyDefinition Hotkey { get; }

    public HotkeyPressedEventArgs(HotkeyDefinition hotkey)
    {
        Hotkey = hotkey;
    }
}
