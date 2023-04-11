using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.Auditing;

public interface IAuditConfigurer
{
    Option<string> Description { get; }

    IImmutableDictionary<string, string> Properties { get; }

    IAuditConfigurer AddProperty(string key, string value);

    IAuditConfigurer RemoveDescription();

    IAuditConfigurer RemoveProperty(string key);

    IAuditConfigurer SetDescription(string description);
}
