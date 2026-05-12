using System.Text;

namespace DndHotkey.Core;

public static class QuietHoursProfileData
{
    public const string PriorityOnlyProfile = "Microsoft.QuietHoursProfile.PriorityOnly";
    public const string UnrestrictedProfile = "Microsoft.QuietHoursProfile.Unrestricted";

    private static readonly byte[] BeforeProfile =
    [
        0x00, 0x00, 0x00, 0x00,
        0x43, 0x42, 0x01, 0x00,
        0xC2, 0x0A, 0x01, 0xD2,
        0x14, 0x28
    ];

    private static readonly byte[] AfterProfile =
    [
        0x00, 0xCA, 0x28, 0x00, 0x00
    ];

    public static byte[] Build(DndState state, DateTimeOffset timestamp)
    {
        var profile = ProfileForState(state);
        var result = new List<byte>(4 + 8 + BeforeProfile.Length + Encoding.Unicode.GetByteCount(profile) + AfterProfile.Length)
        {
            0x02,
            0x00,
            0x00,
            0x00
        };
        result.AddRange(BitConverter.GetBytes(timestamp.ToFileTime()));
        result.AddRange(BeforeProfile);
        result.AddRange(Encoding.Unicode.GetBytes(profile));
        result.AddRange(AfterProfile);
        return result.ToArray();
    }

    public static DndState DetectState(ReadOnlySpan<byte> data)
    {
        if (Contains(data, Encoding.Unicode.GetBytes(PriorityOnlyProfile)))
        {
            return DndState.Enabled;
        }

        if (Contains(data, Encoding.Unicode.GetBytes(UnrestrictedProfile)))
        {
            return DndState.Disabled;
        }

        return DndState.Unknown;
    }

    public static byte[] SetState(ReadOnlySpan<byte> data, DndState state, DateTimeOffset timestamp)
    {
        var currentState = DetectState(data);
        if (currentState is DndState.Unknown)
        {
            return Build(state, timestamp);
        }

        var currentProfile = Encoding.Unicode.GetBytes(ProfileForState(currentState));
        var replacementProfile = Encoding.Unicode.GetBytes(ProfileForState(state));
        var replaced = ReplaceFirst(data, currentProfile, replacementProfile);

        if (replaced.Length >= 12)
        {
            BitConverter.GetBytes(timestamp.ToFileTime()).CopyTo(replaced, 4);
        }

        return replaced;
    }

    private static string ProfileForState(DndState state) =>
        state switch
        {
            DndState.Enabled => PriorityOnlyProfile,
            DndState.Disabled => UnrestrictedProfile,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "State must be enabled or disabled.")
        };

    private static bool Contains(ReadOnlySpan<byte> haystack, ReadOnlySpan<byte> needle) =>
        haystack.IndexOf(needle) >= 0;

    private static byte[] ReplaceFirst(ReadOnlySpan<byte> source, ReadOnlySpan<byte> oldValue, ReadOnlySpan<byte> newValue)
    {
        var index = source.IndexOf(oldValue);
        if (index < 0)
        {
            return source.ToArray();
        }

        var result = new byte[source.Length - oldValue.Length + newValue.Length];
        source[..index].CopyTo(result);
        newValue.CopyTo(result.AsSpan(index));
        source[(index + oldValue.Length)..].CopyTo(result.AsSpan(index + newValue.Length));
        return result;
    }
}
