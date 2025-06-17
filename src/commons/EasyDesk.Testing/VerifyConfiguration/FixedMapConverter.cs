using Argon;
using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Reflection;
using System.Collections.Immutable;

namespace EasyDesk.Testing.VerifyConfiguration;

internal class FixedMapConverter : CachedJsonConverterFactory
{
    protected override JsonConverter CreateConverter(Type objectType)
    {
        var converterType = typeof(FixedMapConverterImpl<,>).MakeGenericType(objectType.GetGenericArguments());
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    public override bool CanConvert(Type type) => type.IsGenericType && type.IsSubtypeOrImplementationOf(typeof(IFixedMap<,>));

    public class FixedMapConverterImpl<K, V> : JsonConverter<IFixedMap<K, V>>
        where K : notnull
    {
        public override void WriteJson(JsonWriter writer, IFixedMap<K, V>? value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value?.AsImmutableDictionary());
        }

        public override IFixedMap<K, V> ReadJson(JsonReader reader, Type type, IFixedMap<K, V>? existingValue, bool hasExisting, JsonSerializer serializer)
        {
            var dictionary = serializer.Deserialize<ImmutableDictionary<K, V>>(reader);
            return FixedHashMap.Create(dictionary);
        }
    }
}
