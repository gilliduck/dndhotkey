using System.Globalization;

namespace DndHotkey.Core;

public static class HotkeyParser
{
    public static HotkeyDefinition Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new HotkeyParseException("Hotkey cannot be empty.");
        }

        var tokens = value
            .Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(token => token.Length > 0)
            .ToArray();

        if (tokens.Length < 2)
        {
            throw new HotkeyParseException("Hotkey must include at least one modifier and one key.");
        }

        var modifiers = HotkeyModifiers.None;
        string? key = null;

        foreach (var token in tokens)
        {
            if (TryParseModifier(token, out var modifier))
            {
                if (modifiers.HasFlag(modifier))
                {
                    throw new HotkeyParseException($"Modifier '{token}' appears more than once.");
                }

                modifiers |= modifier;
                continue;
            }

            if (key is not null)
            {
                throw new HotkeyParseException("Hotkey can contain only one non-modifier key.");
            }

            key = NormalizeKey(token);
        }

        if (modifiers is HotkeyModifiers.None || key is null)
        {
            throw new HotkeyParseException("Hotkey must include at least one modifier and one key.");
        }

        return new HotkeyDefinition(modifiers, key);
    }

    private static bool TryParseModifier(string token, out HotkeyModifiers modifier)
    {
        modifier = token.ToUpperInvariant() switch
        {
            "ALT" => HotkeyModifiers.Alt,
            "CTRL" or "CONTROL" => HotkeyModifiers.Control,
            "SHIFT" => HotkeyModifiers.Shift,
            "WIN" or "WINDOWS" or "META" => HotkeyModifiers.Windows,
            _ => HotkeyModifiers.None
        };
        return modifier is not HotkeyModifiers.None;
    }

    private static string NormalizeKey(string token)
    {
        var normalized = token.Trim();
        if (normalized.Length == 1)
        {
            return normalized.ToUpper(CultureInfo.InvariantCulture);
        }

        var upper = normalized.ToUpperInvariant();
        if (upper.StartsWith('F') &&
            int.TryParse(upper[1..], NumberStyles.None, CultureInfo.InvariantCulture, out var functionKey) &&
            functionKey is >= 1 and <= 24)
        {
            return upper;
        }

        if (upper.All(character => char.IsLetterOrDigit(character) || character is '_'))
        {
            return normalized;
        }

        throw new HotkeyParseException($"'{token}' is not a valid key.");
    }
}
