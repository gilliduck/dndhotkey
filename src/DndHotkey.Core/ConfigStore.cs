using System.Text.Json;

namespace DndHotkey.Core;

public sealed class ConfigStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        WriteIndented = true
    };

    public ConfigStore(string? configDirectory = null)
    {
        ConfigDirectory = configDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DndHotkey");
        ConfigPath = Path.Combine(ConfigDirectory, "config.json");
    }

    public string ConfigDirectory { get; }

    public string ConfigPath { get; }

    public AppConfig LoadOrCreate()
    {
        if (!File.Exists(ConfigPath))
        {
            var defaultConfig = new AppConfig();
            Save(defaultConfig);
            return defaultConfig;
        }

        AppConfig? config;
        try
        {
            var json = File.ReadAllText(ConfigPath);
            config = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions);
        }
        catch (JsonException exception)
        {
            throw new ConfigException($"The config file contains invalid JSON: {ConfigPath}", exception);
        }
        catch (IOException exception)
        {
            throw new ConfigException($"The config file could not be read: {ConfigPath}", exception);
        }

        if (config is null)
        {
            throw new ConfigException($"The config file is empty: {ConfigPath}");
        }

        config.Validate();
        return config;
    }

    public void Save(AppConfig config)
    {
        config.Validate();
        Directory.CreateDirectory(ConfigDirectory);
        var json = JsonSerializer.Serialize(config, JsonOptions);
        File.WriteAllText(ConfigPath, json);
    }
}
