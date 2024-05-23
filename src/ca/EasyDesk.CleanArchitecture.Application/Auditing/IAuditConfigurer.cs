using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Application.Auditing;

public interface IAuditConfigurer
{
    Option<string> Description { get; }

    IFixedMap<string, IFixedSet<string>> Properties { get; }

    IAuditConfigurer AddProperty(string key, string value);

    IAuditConfigurer RemoveDescription();

    IAuditConfigurer RemoveProperty(string key);

    IAuditConfigurer RemoveProperty(string key, string value);

    IAuditConfigurer SetDescription(string description);
}
