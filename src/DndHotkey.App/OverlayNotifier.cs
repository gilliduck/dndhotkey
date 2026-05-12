using System.Windows.Threading;
using DndHotkey.Core;

namespace DndHotkey.App;

internal sealed class OverlayNotifier
{
    private readonly Dispatcher dispatcher;
    private OverlayWindow? currentWindow;
    private DispatcherTimer? closeTimer;

    public OverlayNotifier(Dispatcher dispatcher)
    {
        this.dispatcher = dispatcher;
    }

    public void Show(DndState state, TimeSpan duration, string? customMessage = null)
    {
        dispatcher.Invoke(() =>
        {
            closeTimer?.Stop();
            currentWindow?.Close();

            var message = customMessage ?? state switch
            {
                DndState.Enabled => "Do Not Disturb enabled",
                DndState.Disabled => "Do Not Disturb disabled",
                _ => "Do Not Disturb state unknown"
            };

            currentWindow = new OverlayWindow();
            currentWindow.SetMessage(message, state);
            currentWindow.Show();

            closeTimer = new DispatcherTimer { Interval = duration };
            closeTimer.Tick += (_, _) =>
            {
                closeTimer.Stop();
                currentWindow?.Close();
                currentWindow = null;
            };
            closeTimer.Start();
        });
    }
}
