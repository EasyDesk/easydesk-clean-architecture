using Newtonsoft.Json;

namespace EasyDesk.CleanArchitecture.Application.Json;

public delegate void JsonSettingsConfigurator(JsonSerializerSettings settings);

public static class JsonSettingsConfiguratorExtensions
{
    public static JsonSerializerSettings CreateSettings(
        this JsonSettingsConfigurator configurator,
        JsonSerializerSettings settings = null)
    {
        var actualSettings = settings ?? new JsonSerializerSettings();
        configurator(actualSettings);
        return actualSettings;
    }
}
