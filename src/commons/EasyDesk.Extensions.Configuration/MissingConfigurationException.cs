namespace EasyDesk.Extensions.Configuration;

public sealed class MissingConfigurationException : Exception
{
    public MissingConfigurationException(string key)
        : base($"Missing configuration for key '{key}'")
    {
        Key = key;
    }

    public string Key { get; }
}
