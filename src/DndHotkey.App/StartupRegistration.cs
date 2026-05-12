using Microsoft.Win32;

namespace DndHotkey.App;

internal sealed class StartupRegistration
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "DndHotkey";

    public bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
        var value = key?.GetValue(ValueName) as string;
        return string.Equals(Unquote(value), Environment.ProcessPath, StringComparison.OrdinalIgnoreCase);
    }

    public void Enable()
    {
        using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true);
        key.SetValue(ValueName, Quote(Environment.ProcessPath ?? "DndHotkey.exe"));
    }

    public void Disable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
        key?.DeleteValue(ValueName, throwOnMissingValue: false);
    }

    private static string Quote(string value) => $"\"{value}\"";

    private static string? Unquote(string? value) => value?.Trim().Trim('"');
}
