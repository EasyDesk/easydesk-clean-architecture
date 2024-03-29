﻿using EasyDesk.CleanArchitecture.Application.Abstractions;
using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;
using NodaTime;
using System.Collections.Immutable;

namespace EasyDesk.SampleApp.Application.V_1_0.Queries;

public record AuditRecordDto(
    AuditRecordType Type,
    string Name,
    Option<string> Description,
    Option<AgentDto> Agent,
    IImmutableDictionary<string, IImmutableList<string>> Properties,
    bool Success,
    Instant Instant) : IMappableFrom<AuditRecord, AuditRecordDto>
{
    public static AuditRecordDto MapFrom(AuditRecord src) => new(
        Type: src.Type,
        Name: src.Name,
        Description: src.Description,
        Agent: src.Agent.Map(AgentDto.MapFrom),
        Properties: src.Properties.ToEquatableMap(x => x.Key, x => x.Value.Order().ToEquatableList()),
        Success: src.Success,
        Instant: src.Instant);
}

public record AgentDto(IImmutableDictionary<string, IdentityDto> Identities) : IMappableFrom<Agent, AgentDto>
{
    public static AgentDto MapFrom(Agent src) => new(
        src.Identities.ToImmutableDictionary(x => x.Key.Value, x => IdentityDto.MapFrom(x.Value)));
}

public record IdentityDto(
    string Id,
    IImmutableDictionary<string, IImmutableSet<string>> Attributes) : IMappableFrom<Identity, IdentityDto>
{
    public static IdentityDto MapFrom(Identity src) => new(
        Id: src.Id,
        Attributes: src.Attributes.AttributeMap);
}
