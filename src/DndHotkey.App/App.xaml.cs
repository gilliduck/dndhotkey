using DndHotkey.Core;

namespace DndHotkey.App;

public partial class App : System.Windows.Application
{
    private DndHotkeyTrayApp? trayApp;

    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;

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

    protected override void OnExit(System.Windows.ExitEventArgs e)
    {
        trayApp?.Dispose();
        base.OnExit(e);
    }
}
