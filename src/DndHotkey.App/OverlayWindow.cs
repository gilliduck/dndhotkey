using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using DndHotkey.Core;
using Media = System.Windows.Media;

namespace DndHotkey.App;

internal sealed class OverlayWindow : Window
{
    private readonly Border frame;
    private readonly TextBlock messageText;

    public OverlayWindow()
    {
        Width = 360;
        Height = 96;
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Media.Brushes.Transparent;
        ResizeMode = ResizeMode.NoResize;
        ShowInTaskbar = false;
        Topmost = true;
        IsHitTestVisible = false;
        Focusable = false;

        frame = new Border
        {
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(22, 16, 22, 16),
            BorderThickness = new Thickness(1),
            Effect = new DropShadowEffect
            {
                BlurRadius = 18,
                ShadowDepth = 3,
                Opacity = 0.28
            }
        };
        messageText = new TextBlock
        {
            FontSize = 20,
            FontWeight = FontWeights.SemiBold,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center
        };
        frame.Child = messageText;
        Content = frame;
    }

    public void SetMessage(string message, DndState state)
    {
        messageText.Text = message;
        var enabled = state == DndState.Enabled;
        frame.Background = enabled
            ? new Media.SolidColorBrush(Media.Color.FromRgb(32, 41, 56))
            : new Media.SolidColorBrush(Media.Color.FromRgb(245, 247, 250));
        frame.BorderBrush = enabled
            ? new Media.SolidColorBrush(Media.Color.FromRgb(86, 170, 255))
            : new Media.SolidColorBrush(Media.Color.FromRgb(170, 183, 198));
        messageText.Foreground = enabled
            ? Media.Brushes.White
            : new Media.SolidColorBrush(Media.Color.FromRgb(24, 31, 39));
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Left + (workArea.Width - Width) / 2;
        Top = workArea.Bottom - Height - 56;
    }
}
