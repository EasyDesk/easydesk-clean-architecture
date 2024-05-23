using EasyDesk.CleanArchitecture.Application.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NodaTime;
using NodaTime.Serialization.JsonNet;

namespace EasyDesk.CleanArchitecture.Application.Json;

public static class JsonDefaults
{
    public static void ApplyDefaultConfiguration(this JsonSerializerSettings serializerSettings, IDateTimeZoneProvider? dateTimeZoneProvider = null)
    {
        serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver()
        {
            NamingStrategy = new CamelCaseNamingStrategy()
            {
                ProcessDictionaryKeys = false,
            },
        };

        serializerSettings.Converters.Add(new StringEnumConverter());
        serializerSettings.Converters.Add(new OptionConverter());
        serializerSettings.Converters.Add(new FixedListConverter());
        serializerSettings.Converters.Add(new FixedSetConverter());
        serializerSettings.Converters.Add(new FixedMapConverter());
        serializerSettings.ConfigureForNodaTime(dateTimeZoneProvider ?? DateTimeZoneProviders.Tzdb);
    }

    public static JsonSerializerSettings DefaultSerializerSettings(IDateTimeZoneProvider? dateTimeZoneProvider = null)
    {
        var serializerSettings = new JsonSerializerSettings();
        serializerSettings.ApplyDefaultConfiguration(dateTimeZoneProvider);
        return serializerSettings;
    }
}
