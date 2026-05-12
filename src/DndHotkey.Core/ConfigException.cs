namespace DndHotkey.Core;

public sealed class ConfigException : Exception
{
    public ConfigException(string message)
        : base(message)
    { }

    public ConfigException(string message, Exception innerException)
        : base(message, innerException)
    { }
}
