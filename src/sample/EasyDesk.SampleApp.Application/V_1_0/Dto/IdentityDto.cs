using EasyDesk.CleanArchitecture.Application.Abstractions;
using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.Commons.Collections.Immutable;

namespace EasyDesk.SampleApp.Application.V_1_0.Dto;

public record IdentityDto(
    string Id,
    string Realm,
    IFixedMap<string, IFixedSet<string>> Attributes) : IObjectValue<IdentityDto, Identity>
{
    public static IdentityDto MapFrom(Identity src) => new(
        Id: src.Id,
        Realm: src.Realm,
        Attributes: src.Attributes.AttributeMap);

    public Identity ToDomainObject() => Identity.Create(
        realm: new(Realm),
        id: new(Id),
        attributes: Attributes.SelectMany(x => x.Value.Select(v => (x.Key, v))));
}
