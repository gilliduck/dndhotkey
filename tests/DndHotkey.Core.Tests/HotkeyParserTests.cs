namespace DndHotkey.Core.Tests;

public sealed class HotkeyParserTests
{
    [Fact]
    public void Parse_NormalizesDefaultHotkey()
    {
        var hotkey = HotkeyParser.Parse("Ctrl+Alt+D");

        Assert.Equal(HotkeyModifiers.Control | HotkeyModifiers.Alt, hotkey.Modifiers);
        Assert.Equal("D", hotkey.Key);
        Assert.Equal("Ctrl+Alt+D", hotkey.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("Ctrl+Alt")]
    [InlineData("Ctrl+D+F")]
    [InlineData("Ctrl+Ctrl+D")]
    [InlineData("Banana+D")]
    public void Parse_RejectsInvalidHotkeys(string value)
    {
        Assert.Throws<HotkeyParseException>(() => HotkeyParser.Parse(value));
    }
}
