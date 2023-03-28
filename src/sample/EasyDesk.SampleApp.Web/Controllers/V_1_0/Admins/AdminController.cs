using EasyDesk.CleanArchitecture.Web.Controllers;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.SampleApp.Application.Commands;
using Microsoft.AspNetCore.Mvc;

namespace EasyDesk.SampleApp.Web.Controllers.V_1_0.Admins;

public static class AdminRoutes
{
    public const string AddAdmin = "admin";
}

public class AdminController : CleanArchitectureController
{
    [HttpPost(AdminRoutes.AddAdmin)]
    public async Task<ActionResult<ResponseDto<Nothing, Nothing>>> AddAdmin()
    {
        return await Dispatch(new AddAdmin())
            .ReturnOk();
    }
}
