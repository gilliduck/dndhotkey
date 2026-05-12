using System.Windows;
using DndHotkey.Core;
using Application = System.Windows.Application;

namespace DndHotkey.App;

public partial class App : Application
{
    private DndHotkeyTrayApp? trayApp;

    protected override void OnExit(ExitEventArgs e)
    {
        trayApp?.Dispose();
        base.OnExit(e);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var configStore = new ConfigStore();
        var shellLauncher = new ShellLauncher();
        var dndController = new WindowsDndController();
        var overlayNotifier = new OverlayNotifier(Dispatcher);

        if (CommandLineHandler.TryHandle(e.Args, configStore, dndController, overlayNotifier, shellLauncher))
        {
            Shutdown();
            return;
        }

        trayApp = new DndHotkeyTrayApp(
            configStore,
            dndController,
            () => new WindowsHotkeyRegistrar(),
            overlayNotifier,
            shellLauncher,
            new StartupRegistration());
        trayApp.Start();
    }
}
