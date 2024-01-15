using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;
using System.Collections.Immutable;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Application.Auditing;

public sealed class AuditConfigurer : IAuditConfigurer
{
    public Option<string> Description { get; private set; } = None;

    public IImmutableDictionary<string, IImmutableSet<string>> Properties { get; private set; } = Map<string, IImmutableSet<string>>();

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
        Properties = Properties.Update(key, x => x.Add(value), () => Set(value));
        return this;
    }

    public IAuditConfigurer RemoveProperty(string key)
    {
        Properties = Properties.Remove(key);
        return this;
    }

    public IAuditConfigurer RemoveProperty(string key, string value)
    {
        Properties = Properties.UpdateOption(key, v => v
            .Map(x => x.Remove(value))
            .Filter(x => x.Any()));
        return this;
    }
}
