using Argon;
using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Reflection;
using System.Collections.Immutable;

namespace EasyDesk.Testing.VerifyConfiguration;

internal class FixedListConverter : CachedJsonConverterFactory
{
    protected override JsonConverter CreateConverter(Type objectType)
    {
        var converterType = typeof(FixedListConverterImpl<>).MakeGenericType(objectType.GetGenericArguments());
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    public override bool CanConvert(Type type) => type.IsGenericType && type.IsSubtypeOrImplementationOf(typeof(IFixedList<>));

    public class FixedListConverterImpl<T> : JsonConverter<IFixedList<T>>
    {
        public override void WriteJson(JsonWriter writer, IFixedList<T>? value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value?.AsImmutableList());
        }

        public override IFixedList<T> ReadJson(JsonReader reader, Type type, IFixedList<T>? existingValue, bool hasExisting, JsonSerializer serializer)
        {
            var list = serializer.Deserialize<ImmutableList<T>>(reader);
            return FixedList.Create(list);
        }
    }
}
