using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Reflection;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasyDesk.CleanArchitecture.Application.Json.Converters;

internal class FixedMapConverter : CachedJsonConverterFactory
{
    protected override JsonConverter CreateConverter(Type objectType)
    {
        var converterType = typeof(FixedMapConverterImpl<,>).MakeGenericType(objectType.GetGenericArguments());
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    public override bool CanConvert(Type objectType) =>
        objectType.IsGenericType && objectType.IsSubtypeOrImplementationOf(typeof(IFixedMap<,>));

    public class FixedMapConverterImpl<K, V> : JsonConverter<IFixedMap<K, V>>
        where K : notnull
    {
        public override IFixedMap<K, V>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dictionary = JsonSerializer.Deserialize<ImmutableDictionary<K, V>>(ref reader, options);
            return dictionary is null ? null : FixedHashMap.Create(dictionary);
        }

        public override void Write(Utf8JsonWriter writer, IFixedMap<K, V> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value?.AsImmutableDictionary(), options);
        }
    }
}
