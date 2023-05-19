using EasyDesk.CleanArchitecture.Application.Abstractions;
using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using NodaTime;
using System.Collections.Immutable;

namespace EasyDesk.SampleApp.Application.Queries;

public record AuditRecordDto(
    AuditRecordType Type,
    string Name,
    Option<string> Description,
    Option<UserInfoDto> UserInfo,
    IImmutableDictionary<string, string> Properties,
    bool Success,
    Instant Instant) : IMappableFrom<AuditRecord, AuditRecordDto>
{
    public static AuditRecordDto MapFrom(AuditRecord src) => new(
        Type: src.Type,
        Name: src.Name,
        Description: src.Description,
        UserInfo: src.UserInfo.Map(UserInfoDto.MapFrom),
        Properties: src.Properties,
        Success: src.Success,
        Instant: src.Instant);
}

public record UserInfoDto(
    string UserId,
    IImmutableDictionary<string, IImmutableSet<string>> Attributes) : IMappableFrom<UserInfo, UserInfoDto>
{
    public static UserInfoDto MapFrom(UserInfo src) => new(
        UserId: src.UserId,
        Attributes: src.Attributes.Attributes);
}
