using EasyDesk.CleanArchitecture.Infrastructure.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NodaTime;
using NodaTime.Serialization.JsonNet;

namespace EasyDesk.CleanArchitecture.Infrastructure.Json;

public static class JsonDefaults
{
    public static void ApplyDefaultConfiguration(this JsonSerializerSettings serializerSettings)
    {
        serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        serializerSettings.DateParseHandling = DateParseHandling.None;

        serializerSettings.Converters.Add(new StringEnumConverter());
        serializerSettings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        serializerSettings.Converters.Add(new OptionConverter());
    }

    public static JsonSerializerSettings DefaultSerializerSettings()
    {
        var serializerSettings = new JsonSerializerSettings();
        serializerSettings.ApplyDefaultConfiguration();
        return serializerSettings;
    }
}
