using EasyDesk.CleanArchitecture.Web.Versioning;
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
        _knownTypesByName = knownTypes.ToDictionary(ComputeTypeName);
        _knownNamesByType = _knownTypesByName.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    }

    private string ComputeTypeName(Type type)
    {
        return type
            .GetApiVersionFromNamespace()
            .Match(
                some: v => $"{v}/{type.Name}",
                none: () => type.Name);
    }

    public string GetTypeName(Type type) => _knownNamesByType.GetOption(type).OrElseGet(() => ComputeTypeName(type));

    public Type? GetType(string name) => _knownTypesByName.GetOption(name).OrElseNull();

    public string GetTopic(Type eventType) => GetTypeName(eventType);
}
