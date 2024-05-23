using Argon;
using System.Collections.Concurrent;

namespace EasyDesk.Testing.VerifyConfiguration;

internal abstract class CachedJsonConverterFactory : JsonConverterFactory
{
    private readonly ConcurrentDictionary<Type, JsonConverter> _cache = new();

    protected override JsonConverter CreateConverter(Type objectType, JsonSerializer serializer)
    {
        return _cache.GetOrAdd(objectType, CreateConverter);
    }

    protected abstract JsonConverter CreateConverter(Type objectType);
}
