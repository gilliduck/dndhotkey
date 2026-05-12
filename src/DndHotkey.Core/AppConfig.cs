namespace DndHotkey.Core;

public sealed record AppConfig
{
    public string Hotkey { get; init; } = "Ctrl+Alt+D";

    public bool ShowOverlay { get; init; } = true;

    public int OverlayMilliseconds { get; init; } = 1400;

    public bool StartMinimized { get; init; } = true;

    public void Validate()
    {
        try
        {
            HotkeyParser.Parse(Hotkey);
        }
        catch (HotkeyParseException exception)
        {
            throw new ConfigException($"The configured hotkey is invalid: {exception.Message}", exception);
        }

        if (OverlayMilliseconds is < 250 or > 10000)
        {
            throw new ConfigException("overlayMilliseconds must be between 250 and 10000.");
        }
    }
}
