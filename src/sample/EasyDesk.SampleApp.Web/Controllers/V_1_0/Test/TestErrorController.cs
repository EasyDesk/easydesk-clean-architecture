using EasyDesk.CleanArchitecture.Web.Controllers;
using EasyDesk.CleanArchitecture.Web.Dto;
using Microsoft.AspNetCore.Mvc;

namespace EasyDesk.SampleApp.Web.Controllers.V_1_0.Test;

public class TestErrorController : CleanArchitectureController
{
    public const string V10 = "errors/v1.0";
    public const string V11 = "errors/v1.1";
    public const string Unversioned = "errors/unversioned";

    [HttpGet(V10)]
    public async Task<ActionResult<ResponseDto<Nothing, Nothing>>> GetTestErrorV_1_0()
    {
        return await Dispatch(new Application.V_1_0.Queries.GetTestDomainError())
            .ReturnOk();
    }

    [HttpGet(V11)]
    public async Task<ActionResult<ResponseDto<Nothing, Nothing>>> GetTestErrorV_1_1()
    {
        return await Dispatch(new Application.V_1_1.Queries.GetTestDomainError())
            .ReturnOk();
    }

    [HttpGet(Unversioned)]
    public async Task<ActionResult<ResponseDto<Nothing, Nothing>>> GetTestErrorUnversioned()
    {
        return await Dispatch(new Application.Unversioned.Queries.GetTestDomainError())
            .ReturnOk();
    }
}
