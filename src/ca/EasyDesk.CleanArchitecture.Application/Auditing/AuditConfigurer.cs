using System.Collections.Immutable;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Application.Auditing;

public sealed class AuditConfigurer : IAuditConfigurer
{
    public Option<string> Description { get; private set; } = None;

    public IImmutableDictionary<string, string> Properties { get; private set; } = Map<string, string>();

    public IAuditConfigurer SetDescription(string description)
    {
        Description = Some(description);
        return this;
    }

    public IAuditConfigurer RemoveDescription()
    {
        Description = None;
        return this;
    }

    public IAuditConfigurer AddProperty(string key, string value)
    {
        Properties = Properties.Add(key, value);
        return this;
    }

    public IAuditConfigurer RemoveProperty(string key)
    {
        Properties = Properties.Remove(key);
        return this;
    }
}
