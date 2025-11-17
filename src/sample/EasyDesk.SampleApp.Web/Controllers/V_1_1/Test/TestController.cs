using EasyDesk.CleanArchitecture.Web.Controllers;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.Test;
using Microsoft.AspNetCore.Mvc;

namespace EasyDesk.SampleApp.Web.Controllers.V_1_1.Test;

public class TestController : CleanArchitectureController
{
    public const string Base = "test";

    public const string NodaTimeTestRoute = Base + "/nodatime";

    [HttpPost(NodaTimeTestRoute)]
    public Task<ActionResult<ResponseDto<NodaTimeTestDto, Nothing>>> TestNodaTime()
    {
        return Success(new NodaTimeTestDto());
    }
}
