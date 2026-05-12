using System.Diagnostics;

namespace DndHotkey.App;

internal sealed class ShellLauncher
{
    public void OpenFile(string path)
    {
        Open(path);
    }

    public void OpenFolder(string path)
    {
        Open(path);
    }

    public void OpenNotificationSettings()
    {
        Open("ms-settings:notifications");
    }

    private static void Open(string target)
    {
        Process.Start(new ProcessStartInfo(target)
        {
            UseShellExecute = true
        });
    }
}
