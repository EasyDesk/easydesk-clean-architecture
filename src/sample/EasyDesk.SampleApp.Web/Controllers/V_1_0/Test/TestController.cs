using EasyDesk.CleanArchitecture.Web.Controllers;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.Commons.Options;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using Microsoft.AspNetCore.Mvc;

namespace EasyDesk.SampleApp.Web.Controllers.V_1_0.Test;

public class TestController : CleanArchitectureController
{
    public const string Base = "test";

    public const string TestOptionInQueryObject = Base + "/query/object";

    public const string TestOptionInQuery = Base + "/query";

    public const string ThrowException = Base + "/exception";

    public const string TestPolymorphismRoute = Base + "/polymorphism";

    public const string NodaTimeTestRoute = Base + "/nodatime";

    [HttpGet(TestOptionInQuery)]
    public Task<ActionResult<ResponseDto<Option<string>, Nothing>>> GetValueFromQuery([FromQuery] Option<string> value)
    {
        return Success(value);
    }

    [HttpGet(ThrowException)]
    public Task<ActionResult<ResponseDto<Nothing, Nothing>>> ThrowTestException([FromQuery] Option<string> value)
    {
        throw new InvalidOperationException("Throwing test exception");
    }

    [HttpPost(TestPolymorphismRoute)]
    public Task<ActionResult<ResponseDto<IPolymorphicDto, Nothing>>> TestPolymorphism([FromBody] IPolymorphicDto dto)
    {
        return Success(dto);
    }

    [HttpPost(NodaTimeTestRoute)]
    public Task<ActionResult<ResponseDto<NodaTimeTestDto, Nothing>>> TestNodaTime([FromBody] NodaTimeTestDto body)
    {
        return Success(body);
    }
}
