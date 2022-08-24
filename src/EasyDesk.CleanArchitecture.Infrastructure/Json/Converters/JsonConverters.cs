using Newtonsoft.Json;

namespace EasyDesk.CleanArchitecture.Infrastructure.Json.Converters;

public static class JsonConverters
{
    public static JsonConverter FromStringParser<T>(Func<string, T> parser) => new ParseableStringConverter<T>(parser);
}
