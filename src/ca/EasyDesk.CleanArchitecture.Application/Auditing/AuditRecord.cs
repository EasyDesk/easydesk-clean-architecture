using EasyDesk.CleanArchitecture.Application.ContextProvider;
using NodaTime;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.Auditing;

public enum AuditRecordType
{
    CommandRequest,
    Command,
    Event
}

public record AuditRecord(
    AuditRecordType Type,
    string Name,
    Option<string> Description,
    Option<UserInfo> UserInfo,
    IImmutableDictionary<string, string> Properties,
    bool Success,
    Instant Instant);
