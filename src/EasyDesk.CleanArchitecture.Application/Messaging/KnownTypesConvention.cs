using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using Rebus.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

public class KnownTypesConvention : IMessageTypeNameConvention
{
    private readonly IDictionary<string, Type> _knownTypes;

    public KnownTypesConvention(IEnumerable<Type> knownTypes)
    {
        _knownTypes = knownTypes.ToDictionary(GetTypeName);
    }

    public string GetTypeName(Type type) => type.Name;

    public Type GetType(string name) => _knownTypes.GetOption(name).OrElseNull();
}
