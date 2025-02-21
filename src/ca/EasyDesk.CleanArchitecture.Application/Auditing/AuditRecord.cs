using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Options;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Application.Auditing;

public enum AuditRecordType
{
    CommandRequest,
    Command,
    Event,
}

public record AuditRecord(
    AuditRecordType Type,
    string Name,
    Option<string> Description,
    Option<Agent> Agent,
    IFixedMap<string, IFixedSet<string>> Properties,
    bool Success,
    Instant Instant);
