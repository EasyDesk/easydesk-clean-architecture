using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Reflection;
using Newtonsoft.Json;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.Json.Converters;

internal class FixedListConverter : CachedJsonConverterFactory
{
    protected override JsonConverter CreateConverter(Type objectType)
    {
        var converterType = typeof(FixedListConverterImpl<>).MakeGenericType(objectType.GetGenericArguments());
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    public override bool CanConvert(Type objectType) =>
        objectType.IsGenericType && objectType.IsSubtypeOrImplementationOf(typeof(IFixedList<>));

    public class FixedListConverterImpl<T> : JsonConverter<IFixedList<T>>
    {
        public override void WriteJson(JsonWriter writer, IFixedList<T>? value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value?.AsImmutableList());
        }

        public override IFixedList<T>? ReadJson(JsonReader reader, Type objectType, IFixedList<T>? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var list = serializer.Deserialize<ImmutableList<T>>(reader);
            return list is null ? null : FixedList.Create(list);
        }
    }
}
