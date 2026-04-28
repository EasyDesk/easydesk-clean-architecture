using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Reflection;
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

    public override bool CanConvert(Type typeToConvert)
    {
        if (typeToConvert.IsGenericType && typeToConvert.IsSubtypeOrImplementationOf(typeof(IFixedMap<,>)))
        {
            var genericArguments = typeToConvert.GetGenericArguments();
            if (genericArguments[0] == typeof(string) || genericArguments[0].IsEnum)
            {
                return true;
            }
        }
        return false;
    }

    public class FixedMapConverterImpl<K, V> : JsonConverter<IFixedMap<K, V>>
        where K : notnull
    {
        public override IFixedMap<K, V>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dictionary = JsonSerializer.Deserialize<IDictionary<K, V>?>(ref reader, options);
            return dictionary?.AsEnumerable().ToFixedMap();
        }

        public override void Write(Utf8JsonWriter writer, IFixedMap<K, V> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value.ToSortedDictionary(), options);
        }
    }
}
