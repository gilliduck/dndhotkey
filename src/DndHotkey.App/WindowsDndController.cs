using System.Runtime.InteropServices;
using DndHotkey.Core;
using Microsoft.Win32;

namespace DndHotkey.App;

internal sealed class WindowsDndController : IDndController
{
    private const string DoNotDisturbQuietHoursKey =
        @"Software\Microsoft\Windows\CurrentVersion\CloudStore\Store\DefaultAccount\Current\default$windows.data.donotdisturb.quiethourssettings\windows.data.donotdisturb.quiethourssettings";

    private const string NotificationsQuietHoursKey =
        @"Software\Microsoft\Windows\CurrentVersion\CloudStore\Store\Cache\DefaultAccount\$$windows.data.notifications.quiethourssettings\Current";

    private static readonly string[] ReadKeys =
    [
        NotificationsQuietHoursKey,
        DoNotDisturbQuietHoursKey
    ];

    public DndState GetState()
    {
        foreach (var keyPath in ReadKeys)
        {
            var data = ReadData(keyPath);
            if (data is null)
            {
                continue;
            }

            var state = QuietHoursProfileData.DetectState(data);
            if (state is not DndState.Unknown)
            {
                return state;
            }
        }

        return DndState.Unknown;
    }

    public DndState SetState(DndState state)
    {
        if (state is DndState.Unknown)
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "DND state must be enabled or disabled.");
        }

        var existing = ReadKeys.Select(ReadData).FirstOrDefault(data => data is not null) ?? [];
        var updated = QuietHoursProfileData.SetState(existing, state, DateTimeOffset.UtcNow);

        using var key = Registry.CurrentUser.CreateSubKey(NotificationsQuietHoursKey, true)
                        ?? throw new InvalidOperationException(
                            "Could not open the Windows Do Not Disturb registry key.");
        key.SetValue("Data", updated, RegistryValueKind.Binary);

        BroadcastSettingsChanged();
        return QuietHoursProfileData.DetectState(updated);
    }

    public DndState Toggle()
    {
        var target = GetState() == DndState.Enabled
            ? DndState.Disabled
            : DndState.Enabled;
        return SetState(target);
    }

    private static void BroadcastSettingsChanged()
    {
        _ = SendMessageTimeout(
            new IntPtr(0xFFFF),
            0x001A,
            UIntPtr.Zero,
            "ImmersiveShell",
            0x0002,
            1000,
            out _);
    }

    private static byte[]? ReadData(string keyPath)
    {
        using var key = Registry.CurrentUser.OpenSubKey(keyPath, false);
        return key?.GetValue("Data") as byte[];
    }

    [DllImport("user32.dll", EntryPoint = "SendMessageTimeoutW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr SendMessageTimeout(
        IntPtr hWnd,
        uint msg,
        UIntPtr wParam,
        string lParam,
        uint flags,
        uint timeout,
        out UIntPtr result);
}
