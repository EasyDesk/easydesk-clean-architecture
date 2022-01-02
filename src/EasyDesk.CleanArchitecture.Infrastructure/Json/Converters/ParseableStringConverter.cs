using Newtonsoft.Json;
using System;

namespace EasyDesk.CleanArchitecture.Infrastructure.Json.Converters
{
    public class ParseableStringConverter<T> : JsonConverter<T>
    {
        private readonly Func<string, T> _parser;

        public ParseableStringConverter(Func<string, T> parser)
        {
            _parser = parser;
        }

        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return _parser(reader.Value as string);
        }
    }
}
