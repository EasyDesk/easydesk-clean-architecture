﻿using EasyDesk.Commons.Options;
using Newtonsoft.Json;

namespace EasyDesk.CleanArchitecture.Application.Json.Converters;

internal class OptionConverter : CachedJsonConverterFactory
{
    protected override JsonConverter CreateConverter(Type objectType)
    {
        var converterType = typeof(OptionConverterImpl<>).MakeGenericType(objectType.GetGenericArguments());
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    public override bool CanConvert(Type objectType) =>
        objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Option<>);

    private class OptionConverterImpl<T> : JsonConverter<Option<T>>
    {
        public override void WriteJson(JsonWriter writer, Option<T> value, JsonSerializer serializer)
        {
            value.Match(
                some: t => serializer.Serialize(writer, t),
                none: writer.WriteNull);
        }

        public override Option<T> ReadJson(JsonReader reader, Type objectType, Option<T> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return reader.TokenType == JsonToken.Null ? None : Some(serializer.Deserialize<T>(reader)!);
        }
    }
}
