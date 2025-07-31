using EasyDesk.CleanArchitecture.Application.Versioning;
using EasyDesk.Commons.Collections;
using Rebus.Serialization;
using Rebus.Topic;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging;

internal class KnownTypesConvention : IMessageTypeNameConvention, ITopicNameConvention
{
    private readonly IDictionary<string, Type> _knownTypesByName;
    private readonly IDictionary<Type, string> _knownNamesByType;

    public KnownTypesConvention(IEnumerable<Type> knownTypes)
    {
        _knownTypesByName = knownTypes.ToDictionary(t => t.GetTypeNameWithVersion());
        _knownNamesByType = _knownTypesByName.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    }

    public string GetTypeName(Type type) => _knownNamesByType.GetOption(type).OrElseGet(() => type.GetTypeNameWithVersion());

    public Type? GetType(string name) => _knownTypesByName.GetOption(name).OrElseNull();

    public string GetTopic(Type eventType) => GetTypeName(eventType);
}
