using System.Globalization;

namespace DndHotkey.Core;

public readonly record struct SemanticVersion(int Major, int Minor, int Patch)
{
    public static SemanticVersion Parse(string value)
    {
        var parts = value.Split('.', StringSplitOptions.TrimEntries);
        if (parts.Length != 3)
        {
            throw new FormatException("Semantic versions must use major.minor.patch format.");
        }

        return new SemanticVersion(
            int.Parse(parts[0], CultureInfo.InvariantCulture),
            int.Parse(parts[1], CultureInfo.InvariantCulture),
            int.Parse(parts[2], CultureInfo.InvariantCulture));
    }

    public override string ToString() => $"{Major}.{Minor}.{Patch}";
}
