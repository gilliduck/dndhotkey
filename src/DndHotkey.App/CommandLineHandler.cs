using System.Text;
using System.Windows;
using DndHotkey.Core;
using Directory = System.IO.Directory;

namespace DndHotkey.App;

internal static class CommandLineHandler
{
    public static bool TryHandle(
        string[] args,
        ConfigStore configStore,
        IDndController dndController,
        OverlayNotifier overlayNotifier,
        ShellLauncher shellLauncher)
    {
        if (args.Length == 0)
        {
            return false;
        }

        var options = args.ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (options.Contains("--open-config"))
        {
            configStore.LoadOrCreate();
            shellLauncher.OpenFile(configStore.ConfigPath);
            return true;
        }

        if (options.Contains("--open-config-folder"))
        {
            Directory.CreateDirectory(configStore.ConfigDirectory);
            shellLauncher.OpenFolder(configStore.ConfigDirectory);
            return true;
        }

        if (options.Contains("--diagnose"))
        {
            ShowDiagnostics(configStore, dndController);
            return true;
        }

        DndState? targetState = null;
        if (options.Contains("--enable"))
        {
            targetState = DndState.Enabled;
        }
        else if (options.Contains("--disable"))
        {
            targetState = DndState.Disabled;
        }

        if (targetState is not null || options.Contains("--toggle"))
        {
            var config = configStore.LoadOrCreate();
            var result = targetState is null
                ? dndController.Toggle()
                : dndController.SetState(targetState.Value);

            if (config.ShowOverlay)
            {
                overlayNotifier.Show(result, TimeSpan.FromMilliseconds(config.OverlayMilliseconds));
            }

            return true;
        }

        return false;
    }

    private static void ShowDiagnostics(ConfigStore configStore, IDndController dndController)
    {
        var builder = new StringBuilder()
            .AppendLine("DndHotkey diagnostics")
            .AppendLine()
            .AppendLine($"Config file: {configStore.ConfigPath}")
            .AppendLine($"Config folder: {configStore.ConfigDirectory}")
            .AppendLine($"Detected DND state: {dndController.GetState()}")
            .AppendLine($"Process: {Environment.ProcessPath}");

        System.Windows.MessageBox.Show(builder.ToString(), "DndHotkey", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
