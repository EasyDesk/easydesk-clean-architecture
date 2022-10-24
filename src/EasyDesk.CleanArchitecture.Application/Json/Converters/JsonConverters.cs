using Newtonsoft.Json;

namespace EasyDesk.CleanArchitecture.Application.Json.Converters;

public static class JsonConverters
{
    public static JsonConverter FromStringParser<T>(Func<string, T> parser) => new ParseableStringConverter<T>(parser);
}
