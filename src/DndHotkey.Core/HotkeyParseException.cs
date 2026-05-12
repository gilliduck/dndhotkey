namespace DndHotkey.Core;

public sealed class HotkeyParseException : Exception
{
    public HotkeyParseException(string message)
        : base(message)
    {
    }
}
