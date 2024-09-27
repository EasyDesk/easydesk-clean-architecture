using EasyDesk.CleanArchitecture.Application.Abstractions;
using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Options;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using NodaTime;

namespace EasyDesk.SampleApp.Application.V_1_0.Queries;

public record AuditRecordDto(
    AuditRecordType Type,
    string Name,
    Option<string> Description,
    Option<AgentDto> Agent,
    IFixedMap<string, IFixedList<string>> Properties,
    bool Success,
    Instant Instant) : IMappableFrom<AuditRecord, AuditRecordDto>
{
    public static AuditRecordDto MapFrom(AuditRecord src) => new(
        Type: src.Type,
        Name: src.Name,
        Description: src.Description,
        Agent: src.Agent.Map(AgentDto.MapFrom),
        Properties: src.Properties.ToFixedMap(x => x.Key, x => x.Value.Order().ToFixedList()),
        Success: src.Success,
        Instant: src.Instant);
}
