using EasyDesk.Commons.Options;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.Auditing;

public interface IAuditConfigurer
{
    Option<string> Description { get; }

    IImmutableDictionary<string, IImmutableSet<string>> Properties { get; }

    IAuditConfigurer AddProperty(string key, string value);

    IAuditConfigurer RemoveDescription();

    IAuditConfigurer RemoveProperty(string key);

    IAuditConfigurer RemoveProperty(string key, string value);

    IAuditConfigurer SetDescription(string description);
}
