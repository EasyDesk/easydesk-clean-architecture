using NodaTime;

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
    Option<string> UserId,
    bool Success,
    Instant Instant);
