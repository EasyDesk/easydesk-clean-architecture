using EasyDesk.Tools.Collections;
using Rebus.Serialization;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging;

internal class KnownTypesConvention : IMessageTypeNameConvention
{
    private readonly IDictionary<string, Type> _knownTypes;

    public KnownTypesConvention(KnownMessageTypes knownTypes)
    {
        _knownTypes = knownTypes.Types.ToDictionary(GetTypeName);
    }

    public string GetTypeName(Type type) => type.Name;

    public Type GetType(string name) => _knownTypes.GetOption(name).OrElseNull();
}
