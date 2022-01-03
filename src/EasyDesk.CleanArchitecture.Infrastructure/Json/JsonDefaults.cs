using EasyDesk.CleanArchitecture.Infrastructure.Json.Converters;
using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace EasyDesk.CleanArchitecture.Infrastructure.Json;

public static class JsonDefaults
{
    public static void ApplyDefaultConfiguration(this JsonSerializerSettings serializerSettings)
    {
        serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        serializerSettings.DateParseHandling = DateParseHandling.None;

        serializerSettings.Converters.Add(new StringEnumConverter());
        serializerSettings.Converters.Add(JsonConverters.FromStringParser(Date.Parse));
        serializerSettings.Converters.Add(JsonConverters.FromStringParser(Timestamp.Parse));
        serializerSettings.Converters.Add(JsonConverters.FromStringParser(TimeOfDay.Parse));
        serializerSettings.Converters.Add(JsonConverters.FromStringParser(Duration.Parse));
        serializerSettings.Converters.Add(JsonConverters.FromStringParser(LocalDateTime.Parse));
    }

    public static JsonSerializerSettings DefaultSerializerSettings()
    {
        var serializerSettings = new JsonSerializerSettings();
        serializerSettings.ApplyDefaultConfiguration();
        return serializerSettings;
    }
}
