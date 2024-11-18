using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasyDesk.CleanArchitecture.Application.Json.Converters;

public abstract class CachedJsonConverterFactory : JsonConverterFactory
{
    private readonly ConcurrentDictionary<Type, JsonConverter> _cache = new();

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return _cache.GetOrAdd(typeToConvert, CreateConverter);
    }

    protected abstract JsonConverter CreateConverter(Type objectType);
}
