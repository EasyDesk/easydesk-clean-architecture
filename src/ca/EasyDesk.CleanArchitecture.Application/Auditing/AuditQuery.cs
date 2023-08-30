using EasyDesk.Commons.Options;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Application.Auditing;

public record AuditQuery
{
    public Option<AuditRecordType> MatchType { get; init; } = None;

    public Option<string> MatchName { get; init; } = None;

    public Option<IdentityQuery> MatchIdentity { get; init; } = None;

    public Option<bool> IsAnonymous { get; init; } = None;

    public Option<bool> IsSuccess { get; init; } = None;

    public Option<Instant> FromInstant { get; init; } = None;

    public Option<Instant> ToInstant { get; init; } = None;
}
