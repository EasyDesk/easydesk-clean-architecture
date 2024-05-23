using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace EasyDesk.CleanArchitecture.Application.Json.Converters;

public abstract class CachedJsonConverterFactory : JsonConverterFactory
{
    private readonly ConcurrentDictionary<Type, JsonConverter> _cache = new();

    protected override JsonConverter CreateConverter(Type objectType, JsonSerializer serializer)
    {
        return _cache.GetOrAdd(objectType, CreateConverter);
    }

    protected abstract JsonConverter CreateConverter(Type objectType);
}
