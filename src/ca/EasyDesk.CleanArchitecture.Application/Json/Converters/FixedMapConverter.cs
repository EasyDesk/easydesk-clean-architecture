using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Reflection;
using Newtonsoft.Json;
using System.Collections.Immutable;

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
        public override void WriteJson(JsonWriter writer, IFixedMap<K, V>? value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value?.AsImmutableDictionary());
        }

        public override IFixedMap<K, V>? ReadJson(JsonReader reader, Type objectType, IFixedMap<K, V>? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var dictionary = serializer.Deserialize<ImmutableDictionary<K, V>>(reader);
            return dictionary is null ? null : FixedHashMap.Create(dictionary);
        }
    }
}
