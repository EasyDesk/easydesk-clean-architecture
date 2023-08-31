using EasyDesk.CleanArchitecture.Web.Controllers;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.SampleApp.Application.V_1_1.Queries;
using Microsoft.AspNetCore.Mvc;

namespace EasyDesk.SampleApp.Web.Controllers.V_1_1.Test;

public class TestController : CleanArchitectureController
{
    public const string TestDomainError = "test/domain/error";

    [HttpGet(TestDomainError)]
    public async Task<ActionResult<ResponseDto<Nothing, Nothing>>> GetTestDomainError()
    {
        return await Dispatch(new GetTestDomainError())
            .ReturnOk();
    }
}
