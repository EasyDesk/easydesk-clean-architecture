using EasyDesk.CleanArchitecture.Web.Controllers;
using EasyDesk.CleanArchitecture.Web.Dto;
using Microsoft.AspNetCore.Mvc;

namespace EasyDesk.CleanArchitecture.Web.Versioning;

public sealed class ApiVersioningController : AbstractController
{
    private readonly ApiVersioningInfo _apiVersioningInfo;

    public ApiVersioningController(ApiVersioningInfo apiVersioningInfo)
    {
        _apiVersioningInfo = apiVersioningInfo;
    }

    [HttpGet("versions")]
    public IActionResult GetApiVersions()
    {
        var versionDtos = CreateVersionDtosList(_apiVersioningInfo);
        var responseDto = ResponseDto<IEnumerable<ApiVersionDto>, Nothing>.FromData(versionDtos, Nothing.Value);
        return Ok(responseDto);
    }

    private IEnumerable<ApiVersionDto> CreateVersionDtosList(ApiVersioningInfo apiVersioningInfo)
    {
        return apiVersioningInfo
            .SupportedVersions
            .OrderBy(It)
            .Select(ApiVersionDto.FromApiVersion);
    }
}
