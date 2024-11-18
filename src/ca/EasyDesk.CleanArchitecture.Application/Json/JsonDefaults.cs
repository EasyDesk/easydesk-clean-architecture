using EasyDesk.CleanArchitecture.Application.Json.Converters;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasyDesk.CleanArchitecture.Application.Json;

public static class JsonDefaults
{
    public static void ApplyDefaultConfiguration(this JsonSerializerOptions options, IDateTimeZoneProvider? dateTimeZoneProvider = null)
    {
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

        options.Converters.Add(new JsonStringEnumConverter());
        options.Converters.Add(new OptionConverter());
        options.Converters.Add(new FixedListConverter());
        options.Converters.Add(new FixedSetConverter());
        options.Converters.Add(new FixedMapConverter());
        options.ConfigureForNodaTime(dateTimeZoneProvider ?? DateTimeZoneProviders.Tzdb);
    }

    public static JsonSerializerOptions DefaultSerializerOptions(IDateTimeZoneProvider? dateTimeZoneProvider = null)
    {
        var serializerSettings = new JsonSerializerOptions();
        serializerSettings.ApplyDefaultConfiguration(dateTimeZoneProvider);
        return serializerSettings;
    }
}
