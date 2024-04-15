using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.Commons.Options;
using NodaTime;
using System.Collections.Immutable;

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
    IImmutableDictionary<string, IImmutableSet<string>> Properties,
    bool Success,
    Instant Instant);
