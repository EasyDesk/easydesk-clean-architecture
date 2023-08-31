using EasyDesk.CleanArchitecture.Application.Versioning;

namespace EasyDesk.CleanArchitecture.Web.Dto;

public record ApiVersionDto(string Version)
{
    public static ApiVersionDto FromApiVersion(ApiVersion apiVersion) =>
        new(apiVersion.ToStringWithoutV());
}
