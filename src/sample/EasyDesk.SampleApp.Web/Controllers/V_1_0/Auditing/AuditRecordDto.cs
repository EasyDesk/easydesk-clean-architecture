using EasyDesk.CleanArchitecture.Application.Abstractions;
using EasyDesk.CleanArchitecture.Application.Auditing;
using NodaTime;

namespace EasyDesk.SampleApp.Web.Controllers.V_1_0.Auditing;

public record AuditRecordDto(
    AuditRecordType Type,
    string Name,
    Option<string> Description,
    Option<string> UserId,
    bool Success,
    Instant Instant) : IMappableFrom<AuditRecord, AuditRecordDto>
{
    public static AuditRecordDto MapFrom(AuditRecord src) => new(
        Type: src.Type,
        Name: src.Name,
        Description: src.Description,
        UserId: src.UserId,
        Success: src.Success,
        Instant: src.Instant);
}
