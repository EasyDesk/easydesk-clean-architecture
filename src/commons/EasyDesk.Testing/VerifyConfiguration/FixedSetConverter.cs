using Argon;
using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Reflection;
using System.Collections.Immutable;

namespace EasyDesk.Testing.VerifyConfiguration;

internal class FixedSetConverter : CachedJsonConverterFactory
{
    protected override JsonConverter CreateConverter(Type objectType)
    {
        var converterType = typeof(FixedSetConverterImpl<>).MakeGenericType(objectType.GetGenericArguments());
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    public override bool CanConvert(Type objectType) =>
        objectType.IsGenericType && objectType.IsSubtypeOrImplementationOf(typeof(IFixedSet<>));

    public class FixedSetConverterImpl<T> : JsonConverter<IFixedSet<T>>
    {
        public override void WriteJson(JsonWriter writer, IFixedSet<T>? value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value?.AsImmutableSet());
        }

        public override IFixedSet<T> ReadJson(JsonReader reader, Type objectType, IFixedSet<T>? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var set = serializer.Deserialize<ImmutableHashSet<T>>(reader);
            return FixedHashSet.Create(set);
        }
    }
}
