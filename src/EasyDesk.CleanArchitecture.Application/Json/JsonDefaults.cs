using EasyDesk.CleanArchitecture.Application.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NodaTime;
using NodaTime.Serialization.JsonNet;

namespace EasyDesk.CleanArchitecture.Application.Json;

public static class JsonDefaults
{
    public static void ApplyDefaultConfiguration(this JsonSerializerSettings serializerSettings, IDateTimeZoneProvider dateTimeZoneProvider = null)
    {
        serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

        serializerSettings.Converters.Add(new StringEnumConverter());
        serializerSettings.Converters.Add(new OptionConverter());
        serializerSettings.ConfigureForNodaTime(dateTimeZoneProvider ?? DateTimeZoneProviders.Tzdb);
    }

    public static JsonSerializerSettings DefaultSerializerSettings(IDateTimeZoneProvider dateTimeZoneProvider = null)
    {
        var serializerSettings = new JsonSerializerSettings();
        serializerSettings.ApplyDefaultConfiguration(dateTimeZoneProvider);
        return serializerSettings;
    }
}
