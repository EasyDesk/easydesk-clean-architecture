﻿using Argon;
using EasyDesk.Commons.Options;

namespace EasyDesk.Testing.VerifyConfiguration;

internal class OptionConverter : JsonConverterFactory
{
    protected override JsonConverter CreateConverter(Type objectType, JsonSerializer serializer)
    {
        var converterType = typeof(OptionConverterImpl<>).MakeGenericType(objectType.GetGenericArguments());
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    public override bool CanConvert(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Option<>);

    private class OptionConverterImpl<T> : JsonConverter<Option<T>>
    {
        public override void WriteJson(JsonWriter writer, Option<T> value, JsonSerializer serializer)
        {
            value.Match(
                some: t => serializer.Serialize(writer, t),
                none: writer.WriteNull);
        }

        public override Option<T> ReadJson(JsonReader reader, Type type, Option<T> existingValue, bool hasExisting, JsonSerializer serializer)
        {
            return reader.TokenType == JsonToken.Null ? None : Some(serializer.Deserialize<T>(reader));
        }
    }
}

internal class NoneOptionConverter : JsonConverter<NoneOption>
{
    public override NoneOption ReadJson(JsonReader reader, Type type, NoneOption existingValue, bool hasExisting, JsonSerializer serializer)
    {
        return None;
    }

    public override void WriteJson(JsonWriter writer, NoneOption value, JsonSerializer serializer)
    {
        writer.WriteNull();
    }
}
