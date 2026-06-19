using EasyDesk.CleanArchitecture.Web.Controllers;
using EasyDesk.CleanArchitecture.Web.Dto;
using Microsoft.AspNetCore.Mvc;

namespace EasyDesk.SampleApp.Web.Controllers.V_0_1;

public class OldApiController : CleanArchitectureController
{
    [HttpGet("old-api")]
    public async Task<ActionResult<ResponseDto<string, Nothing>>> OldApi()
    {
        return await Success("hello");
    }
}
