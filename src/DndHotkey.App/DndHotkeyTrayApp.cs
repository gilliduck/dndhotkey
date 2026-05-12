using System.Windows;
using DndHotkey.Core;
using Directory = System.IO.Directory;
using WinForms = System.Windows.Forms;

namespace DndHotkey.App;

internal sealed class DndHotkeyTrayApp : IDisposable
{
    private readonly ConfigStore configStore;
    private readonly IDndController dndController;
    private readonly Func<IHotkeyRegistrar> hotkeyRegistrarFactory;
    private readonly OverlayNotifier overlayNotifier;
    private readonly ShellLauncher shellLauncher;
    private readonly StartupRegistration startupRegistration;
    private WinForms.NotifyIcon? notifyIcon;
    private IHotkeyRegistrar? hotkeyRegistrar;
    private AppConfig config = new();

    public DndHotkeyTrayApp(
        ConfigStore configStore,
        IDndController dndController,
        Func<IHotkeyRegistrar> hotkeyRegistrarFactory,
        OverlayNotifier overlayNotifier,
        ShellLauncher shellLauncher,
        StartupRegistration startupRegistration)
    {
        this.configStore = configStore;
        this.dndController = dndController;
        this.hotkeyRegistrarFactory = hotkeyRegistrarFactory;
        this.overlayNotifier = overlayNotifier;
        this.shellLauncher = shellLauncher;
        this.startupRegistration = startupRegistration;
    }

    public void Start()
    {
        config = LoadConfigOrDefault();
        RegisterHotkey();
        notifyIcon = CreateNotifyIcon();
        UpdateNotifyIconText();
    }

    public void Dispose()
    {
        hotkeyRegistrar?.Dispose();
        if (notifyIcon is not null)
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
        }
    }

    private AppConfig LoadConfigOrDefault()
    {
        try
        {
            return configStore.LoadOrCreate();
        }
        catch (ConfigException exception)
        {
            System.Windows.MessageBox.Show(
                $"{exception.Message}\n\nDndHotkey will keep running with default settings until the config is fixed and reloaded.",
                "DndHotkey configuration",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            shellLauncher.OpenFile(configStore.ConfigPath);
            return new AppConfig();
        }
    }

    private void RegisterHotkey()
    {
        hotkeyRegistrar?.Dispose();
        hotkeyRegistrar = hotkeyRegistrarFactory();
        hotkeyRegistrar.HotkeyPressed += OnHotkeyPressed;
        hotkeyRegistrar.Register(HotkeyParser.Parse(config.Hotkey));
    }

    private WinForms.NotifyIcon CreateNotifyIcon()
    {
        var contextMenu = new WinForms.ContextMenuStrip();
        contextMenu.Items.Add("Toggle Do Not Disturb", null, (_, _) => ToggleDnd());
        contextMenu.Items.Add("Open config file", null, (_, _) => OpenConfigFile());
        contextMenu.Items.Add("Open config folder", null, (_, _) => OpenConfigFolder());
        contextMenu.Items.Add("Reload config", null, (_, _) => ReloadConfig());
        contextMenu.Items.Add(new WinForms.ToolStripSeparator());

        var startupItem = new WinForms.ToolStripMenuItem("Start with Windows")
        {
            CheckOnClick = true,
            Checked = startupRegistration.IsEnabled()
        };
        startupItem.Click += (_, _) =>
        {
            if (startupItem.Checked)
            {
                startupRegistration.Enable();
            }
            else
            {
                startupRegistration.Disable();
            }
        };
        contextMenu.Items.Add(startupItem);
        contextMenu.Items.Add(new WinForms.ToolStripSeparator());
        contextMenu.Items.Add("Quit", null, (_, _) => System.Windows.Application.Current.Shutdown());

        var icon = new WinForms.NotifyIcon
        {
            ContextMenuStrip = contextMenu,
            Icon = System.Drawing.SystemIcons.Application,
            Visible = true
        };
        icon.MouseClick += (_, args) =>
        {
            if (args.Button == WinForms.MouseButtons.Left)
            {
                ToggleDnd();
            }
        };
        return icon;
    }

    private void OnHotkeyPressed(object? sender, HotkeyPressedEventArgs e)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(ToggleDnd);
    }

    private void ToggleDnd()
    {
        try
        {
            var state = dndController.Toggle();
            UpdateNotifyIconText();
            if (config.ShowOverlay)
            {
                overlayNotifier.Show(state, TimeSpan.FromMilliseconds(config.OverlayMilliseconds));
            }
        }
        catch (Exception exception)
        {
            System.Windows.MessageBox.Show(
                $"{exception.Message}\n\nWindows notification settings will open so you can toggle Do Not Disturb manually.",
                "DndHotkey",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            shellLauncher.OpenNotificationSettings();
        }
    }

    private void OpenConfigFile()
    {
        configStore.LoadOrCreate();
        shellLauncher.OpenFile(configStore.ConfigPath);
    }

    private void OpenConfigFolder()
    {
        Directory.CreateDirectory(configStore.ConfigDirectory);
        shellLauncher.OpenFolder(configStore.ConfigDirectory);
    }

    private void ReloadConfig()
    {
        config = LoadConfigOrDefault();
        RegisterHotkey();
        overlayNotifier.Show(DndState.Unknown, TimeSpan.FromMilliseconds(900), "Config reloaded");
        UpdateNotifyIconText();
    }

    private void UpdateNotifyIconText()
    {
        if (notifyIcon is null)
        {
            return;
        }

        notifyIcon.Text = dndController.GetState() switch
        {
            DndState.Enabled => "DndHotkey - Do Not Disturb enabled",
            DndState.Disabled => "DndHotkey - Do Not Disturb disabled",
            _ => "DndHotkey - Do Not Disturb state unknown"
        };
    }
}
