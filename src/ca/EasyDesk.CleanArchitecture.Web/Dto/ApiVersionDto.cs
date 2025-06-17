using EasyDesk.CleanArchitecture.Application.Versioning;

namespace EasyDesk.CleanArchitecture.Web.Dto;

public record ApiVersionDto
{
    public required string Version { get; init; }

    public static ApiVersionDto FromApiVersion(ApiVersion apiVersion) =>
        new() { Version = apiVersion.ToStringWithoutV(), };
}
