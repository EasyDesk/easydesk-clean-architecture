using EasyDesk.CleanArchitecture.Web.Controllers;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.SampleApp.Application.Commands;
using Microsoft.AspNetCore.Mvc;

namespace EasyDesk.SampleApp.Web.Controllers.V_1_0.Admins;

public static class AdminRoutes
{
    public const string Admin = "admin";

    public const string Roles = "roles";
}

public class AdminController : CleanArchitectureController
{
    [HttpPost(AdminRoutes.Admin)]
    public async Task<ActionResult<ResponseDto<Nothing, Nothing>>> AddAdmin()
    {
        return await Dispatch(new AddAdmin())
            .ReturnOk();
    }

    [HttpDelete(AdminRoutes.Admin)]
    public async Task<ActionResult<ResponseDto<Nothing, Nothing>>> RemoveAdmin()
    {
        return await Dispatch(new RemoveAdmin())
            .ReturnOk();
    }

    [HttpDelete(AdminRoutes.Roles)]
    public async Task<ActionResult<ResponseDto<Nothing, Nothing>>> RemoveRoles()
    {
        return await Dispatch(new RemoveRoles())
            .ReturnOk();
    }
}
