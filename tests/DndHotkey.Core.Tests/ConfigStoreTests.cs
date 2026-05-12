namespace DndHotkey.Core.Tests;

public sealed class ConfigStoreTests
{
    [Fact]
    public void LoadOrCreate_CreatesDefaultConfigWhenMissing()
    {
        using var directory = TempDirectory.Create();
        var store = new ConfigStore(directory.Path);

        var config = store.LoadOrCreate();

        Assert.Equal("Ctrl+Alt+D", config.Hotkey);
        Assert.True(config.ShowOverlay);
        Assert.Equal(1400, config.OverlayMilliseconds);
        Assert.True(config.StartMinimized);
        Assert.True(File.Exists(store.ConfigPath));
    }

    [Fact]
    public void LoadOrCreate_WritesReadableDefaultHotkey()
    {
        using var directory = TempDirectory.Create();
        var store = new ConfigStore(directory.Path);

        store.LoadOrCreate();

        var json = File.ReadAllText(store.ConfigPath);
        Assert.Contains("\"hotkey\": \"Ctrl+Alt+D\"", json);
        Assert.DoesNotContain("\\u002B", json);
    }

    [Fact]
    public void LoadOrCreate_ReadsExistingConfig()
    {
        using var directory = TempDirectory.Create();
        var store = new ConfigStore(directory.Path);
        Directory.CreateDirectory(directory.Path);
        File.WriteAllText(
            store.ConfigPath,
            """
            {
              "hotkey": "Ctrl+Shift+F12",
              "showOverlay": false,
              "overlayMilliseconds": 2500,
              "startMinimized": false
            }
            """);

        var config = store.LoadOrCreate();

        Assert.Equal("Ctrl+Shift+F12", config.Hotkey);
        Assert.False(config.ShowOverlay);
        Assert.Equal(2500, config.OverlayMilliseconds);
        Assert.False(config.StartMinimized);
    }

    [Fact]
    public void LoadOrCreate_RejectsInvalidHotkey()
    {
        using var directory = TempDirectory.Create();
        var store = new ConfigStore(directory.Path);
        Directory.CreateDirectory(directory.Path);
        File.WriteAllText(
            store.ConfigPath,
            """
            {
              "hotkey": "Ctrl+Alt",
              "showOverlay": true,
              "overlayMilliseconds": 1400,
              "startMinimized": true
            }
            """);

        Assert.Throws<ConfigException>(() => store.LoadOrCreate());
    }

    [Fact]
    public void LoadOrCreate_RejectsInvalidJson()
    {
        using var directory = TempDirectory.Create();
        var store = new ConfigStore(directory.Path);
        Directory.CreateDirectory(directory.Path);
        File.WriteAllText(store.ConfigPath, "{ nope");

        var exception = Assert.Throws<ConfigException>(() => store.LoadOrCreate());

        Assert.Contains("invalid JSON", exception.Message);
    }
}
