using EasyDesk.CleanArchitecture.Application.Abstractions;
using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Collections.Immutable;

namespace EasyDesk.SampleApp.Application.V_1_0.Dto;

public record AgentDto(IFixedList<IdentityDto> Identities) : IObjectValue<AgentDto, Agent>
{
    public static AgentDto MapFrom(Agent src) => new(
        src.Identities.Select(x => IdentityDto.MapFrom(x.Value)).ToFixedList());

    public Agent ToDomainObject() => Agent.FromIdentities(Identities.Select(x => x.ToDomainObject()));
}
