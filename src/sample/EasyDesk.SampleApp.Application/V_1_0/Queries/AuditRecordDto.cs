using EasyDesk.CleanArchitecture.Application.Abstractions;
using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Options;
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

public record AgentDto(IFixedMap<string, IdentityDto> Identities) : IMappableFrom<Agent, AgentDto>
{
    public static AgentDto MapFrom(Agent src) => new(
        src.Identities.ToFixedMap(x => x.Key.Value, x => IdentityDto.MapFrom(x.Value)));
}

public record IdentityDto(
    string Id,
    IFixedMap<string, IFixedSet<string>> Attributes) : IMappableFrom<Identity, IdentityDto>
{
    public static IdentityDto MapFrom(Identity src) => new(
        Id: src.Id,
        Attributes: src.Attributes.AttributeMap);
}
