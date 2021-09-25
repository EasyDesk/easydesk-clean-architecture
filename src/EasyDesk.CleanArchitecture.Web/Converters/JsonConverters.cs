using Newtonsoft.Json;
using System;

namespace EasyDesk.CleanArchitecture.Web.Converters
{
    public static class JsonConverters
    {
        public static JsonConverter FromStringParser<T>(Func<string, T> parser) => new ParseableStringConverter<T>(parser);
    }
}
