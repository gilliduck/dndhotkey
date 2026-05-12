namespace DndHotkey.Core;

public sealed record HotkeyDefinition(HotkeyModifiers Modifiers, string Key)
{
    public override string ToString()
    {
        var parts = new List<string>();
        AppendIfSet(parts, HotkeyModifiers.Control, "Ctrl");
        AppendIfSet(parts, HotkeyModifiers.Shift, "Shift");
        AppendIfSet(parts, HotkeyModifiers.Alt, "Alt");
        AppendIfSet(parts, HotkeyModifiers.Windows, "Win");
        parts.Add(Key);
        return string.Join("+", parts);
    }

    private void AppendIfSet(List<string> parts, HotkeyModifiers modifier, string label)
    {
        if (Modifiers.HasFlag(modifier))
        {
            parts.Add(label);
        }
    }
}
