using NodaTime;

namespace EasyDesk.CleanArchitecture.Application.Auditing;

public record AuditQuery
{
    public Option<AuditRecordType> MatchType { get; init; } = None;

    public Option<string> MatchName { get; init; } = None;

    public Option<string> MatchUserId { get; init; } = None;

    public Option<bool> IsAnonymous { get; init; } = None;

    public Option<bool> IsSuccess { get; init; } = None;

    public Interval MatchTimeInterval { get; init; } = new Interval(null, null);
}
