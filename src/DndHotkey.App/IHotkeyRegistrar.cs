using DndHotkey.Core;

namespace DndHotkey.App;

internal interface IHotkeyRegistrar : IDisposable
{
    event EventHandler<HotkeyPressedEventArgs>? HotkeyPressed;

    void Register(HotkeyDefinition hotkey);
}
