using EasyDesk.CleanArchitecture.Application.Abstractions;
using EasyDesk.CleanArchitecture.Application.Auditing;
using NodaTime;
using System.Collections.Immutable;

namespace EasyDesk.SampleApp.Web.Controllers.V_1_0.Auditing;

public record AuditRecordDto(
    AuditRecordType Type,
    string Name,
    Option<string> Description,
    Option<string> UserId,
    IImmutableDictionary<string, string> Properties,
    bool Success,
    Instant Instant) : IMappableFrom<AuditRecord, AuditRecordDto>
{
    public static AuditRecordDto MapFrom(AuditRecord src) => new(
        Type: src.Type,
        Name: src.Name,
        Description: src.Description,
        UserId: src.UserId.Map(u => u.Value),
        Properties: src.Properties,
        Success: src.Success,
        Instant: src.Instant);
}
