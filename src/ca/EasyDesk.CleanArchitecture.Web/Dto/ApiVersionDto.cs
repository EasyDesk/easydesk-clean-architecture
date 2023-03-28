using Microsoft.AspNetCore.Mvc;

namespace EasyDesk.CleanArchitecture.Web.Dto;

public record ApiVersionDto(string Version)
{
    public static ApiVersionDto FromApiVersion(ApiVersion apiVersion) =>
        new(apiVersion.ToString());
}
