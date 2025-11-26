using EasyDesk.Commons.Reflection;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasyDesk.CleanArchitecture.Application.Json.Converters;

internal class DictionaryOfAnyKeyConverter : CachedJsonConverterFactory
{
    protected override JsonConverter CreateConverter(Type objectType)
    {
        var converterType = objectType.IsSubtypeOrImplementationOf(typeof(IImmutableDictionary<,>))
            ? typeof(ImmutableDictionaryAsArrayConverterImpl<,>)
            : typeof(DictionaryAsArrayConverterImpl<,>);
        var converterTypeWithGenericArguments = converterType.MakeGenericType(objectType.GetGenericArguments());
        return (JsonConverter)Activator.CreateInstance(converterTypeWithGenericArguments)!;
    }

    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsGenericType
            && (typeToConvert.IsSubtypeOrImplementationOf(typeof(IImmutableDictionary<,>))
                || typeToConvert.IsSubtypeOrImplementationOf(typeof(IDictionary<,>)))
            && typeToConvert.GetGenericArguments()[0] != typeof(string);

    public class ImmutableDictionaryAsArrayConverterImpl<K, V> : JsonConverter<IImmutableDictionary<K, V>>
        where K : notnull
    {
        public override IImmutableDictionary<K, V>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var list = JsonSerializer.Deserialize<ImmutableArray<KeyValuePair<K, V>>?>(ref reader, options);
            return list?.ToImmutableDictionary();
        }

        public override void Write(Utf8JsonWriter writer, IImmutableDictionary<K, V> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value?.ToImmutableArray(), options);
        }
    }

    public class DictionaryAsArrayConverterImpl<K, V> : JsonConverter<IDictionary<K, V>>
        where K : notnull
    {
        public override IDictionary<K, V>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var list = JsonSerializer.Deserialize<ImmutableArray<KeyValuePair<K, V>>?>(ref reader, options);
            return list?.ToDictionary();
        }

        public override void Write(Utf8JsonWriter writer, IDictionary<K, V> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value?.ToArray(), options);
        }
    }
}
