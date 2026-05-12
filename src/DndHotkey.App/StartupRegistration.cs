using Microsoft.Win32;

namespace DndHotkey.App;

internal sealed class StartupRegistration
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "DndHotkey";

    public void Disable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);
        key?.DeleteValue(ValueName, false);
    }

    public void Enable()
    {
        using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath, true);
        key.SetValue(ValueName, Quote(Environment.ProcessPath ?? "DndHotkey.exe"));
    }

    public bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false);
        var value = key?.GetValue(ValueName) as string;
        return string.Equals(Unquote(value), Environment.ProcessPath, StringComparison.OrdinalIgnoreCase);
    }

    private static string Quote(string value)
    {
        return $"\"{value}\"";
    }

    private static string? Unquote(string? value)
    {
        return value?.Trim().Trim('"');
    }
}
