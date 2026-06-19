using EasyDesk.CleanArchitecture.Application.Json.Converters;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

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
        options.Converters.Add(new FixedMapOfAnyKeyConverter());
        options.Converters.Add(new DictionaryOfAnyKeyConverter());
        options.ConfigureForNodaTime(dateTimeZoneProvider ?? DateTimeZoneProviders.Tzdb);
    }

    public static JsonSerializerOptions DefaultSerializerOptions(IDateTimeZoneProvider? dateTimeZoneProvider = null)
    {
        var serializerSettings = new JsonSerializerOptions();
        serializerSettings.ApplyDefaultConfiguration(dateTimeZoneProvider);
        return serializerSettings;
    }

    /// <summary>
    /// Returns the list of derived types, optionally including the type itself if it is not abstract and not already specified as a derived type.
    /// </summary>
    /// <param name="typeInfo">The JSON type info for which to get derived types.</param>
    /// <returns>An enumerable of derived types.</returns>
    public static IEnumerable<JsonDerivedType> GetDerivedTypes(this JsonTypeInfo typeInfo)
    {
        if (typeInfo.PolymorphismOptions is null)
        {
            return [];
        }
        var derivedTypes = new List<JsonDerivedType>(typeInfo.PolymorphismOptions.DerivedTypes);
        if (!typeInfo.Type.IsAbstract && !typeInfo.IsPolymorphicTypeThatSpecifiesItselfAsDerivedType())
        {
            derivedTypes.Add(new JsonDerivedType(typeInfo.Type));
        }
        return derivedTypes;
    }

    public static bool IsPolymorphicTypeThatSpecifiesItselfAsDerivedType(this JsonTypeInfo typeInfo)
    {
        foreach (var derivedType in typeInfo.PolymorphismOptions?.DerivedTypes ?? [])
        {
            if (derivedType.DerivedType == typeInfo.Type)
            {
                return true;
            }
        }

        return false;
    }
}
