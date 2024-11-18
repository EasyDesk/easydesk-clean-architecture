using System.Text.Json;

namespace EasyDesk.CleanArchitecture.Application.Json;

public delegate void JsonOptionsConfigurator(JsonSerializerOptions options);

public static class JsonOptionsConfiguratorExtensions
{
    public static JsonSerializerOptions CreateOptions(
        this JsonOptionsConfigurator configurator,
        JsonSerializerOptions? settings = null)
    {
        var actualSettings = settings ?? new JsonSerializerOptions();
        configurator(actualSettings);
        return actualSettings;
    }
}
