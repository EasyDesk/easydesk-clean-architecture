using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.CleanArchitecture.Web.Versioning;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace EasyDesk.CleanArchitecture.Web.Controllers;

public abstract class VersionsController : AbstractController
{
    private readonly Type[] _assemblyTypes;

    public VersionsController(params Type[] assemblyTypes)
    {
        _assemblyTypes = assemblyTypes;
    }

    [HttpGet("versions")]
    public IActionResult GetSupportedVersions()
    {
        var versions = VersioningUtils.GetSupportedVersions(_assemblyTypes)
            .Select(v => new SupportedVersionDto(v.ToString()));

        return Ok(ResponseDto.FromData(versions));
    }
}
