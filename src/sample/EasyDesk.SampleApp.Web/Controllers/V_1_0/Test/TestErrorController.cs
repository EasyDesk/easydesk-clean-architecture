using EasyDesk.CleanArchitecture.Web.Controllers;
using EasyDesk.CleanArchitecture.Web.Dto;
using Microsoft.AspNetCore.Mvc;

namespace EasyDesk.SampleApp.Web.Controllers.V_1_0.Test;

public class TestErrorController : CleanArchitectureController
{
    public const string V01 = "errors/v0.1";
    public const string V10 = "errors/v1.0";
    public const string V11 = "errors/v1.1";
    public const string V15 = "errors/v1.5";
    public const string Unversioned = "errors/unversioned";

    [HttpGet(V01)]
    public async Task<ActionResult<ResponseDto<Nothing, Nothing>>> GetTestErrorV_0_1()
    {
        return await Dispatch(new Application.V_0_1.Queries.GetTestDomainError())
            .ReturnOk();
    }

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

    [HttpGet(V15)]
    public async Task<ActionResult<ResponseDto<Nothing, Nothing>>> GetTestErrorV_1_5()
    {
        return await Dispatch(new Application.V_1_5.Queries.GetTestDomainError())
            .ReturnOk();
    }

    [HttpGet(Unversioned)]
    public async Task<ActionResult<ResponseDto<Nothing, Nothing>>> GetTestErrorUnversioned()
    {
        return await Dispatch(new Application.Unversioned.Queries.GetTestDomainError())
            .ReturnOk();
    }
}
