using EasyDesk.CleanArchitecture.Application.Versioning;

namespace EasyDesk.CleanArchitecture.Web.Versioning;

public static class RestApiVersioning
{
    public const string VersionHeader = "api-version";
    public const string VersionQueryParam = "version";

    public static Microsoft.AspNetCore.Mvc.ApiVersion ToAspNetApiVersion(this ApiVersion version) =>
        new((int)version.Major, (int)version.Minor);
}
