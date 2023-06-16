using EasyDesk.CleanArchitecture.Application.Abstractions;
using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using NodaTime;
using System.Collections.Immutable;

namespace EasyDesk.SampleApp.Application.V_1_0.Queries;

public record AuditRecordDto(
    AuditRecordType Type,
    string Name,
    Option<string> Description,
    Option<AgentDto> Agent,
    IImmutableDictionary<string, string> Properties,
    bool Success,
    Instant Instant) : IMappableFrom<AuditRecord, AuditRecordDto>
{
    public static AuditRecordDto MapFrom(AuditRecord src) => new(
        Type: src.Type,
        Name: src.Name,
        Description: src.Description,
        Agent: src.Agent.Map(AgentDto.MapFrom),
        Properties: src.Properties,
        Success: src.Success,
        Instant: src.Instant);
}

public record AgentDto(IImmutableDictionary<string, IdentityDto> Identities) : IMappableFrom<Agent, AgentDto>
{
    public static AgentDto MapFrom(Agent src) => new(
        src.Identities.ToImmutableDictionary(x => x.Key, x => IdentityDto.MapFrom(x.Value)));
}

public record IdentityDto(
    string Id,
    IImmutableDictionary<string, IImmutableSet<string>> Attributes) : IMappableFrom<Identity, IdentityDto>
{
    public static IdentityDto MapFrom(Identity src) => new(
        Id: src.Id,
        Attributes: src.Attributes.Attributes);
}
