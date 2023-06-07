using EasyDesk.CleanArchitecture.Web.Controllers;
using EasyDesk.CleanArchitecture.Web.Dto;
using Microsoft.AspNetCore.Mvc;

namespace EasyDesk.SampleApp.Web.Controllers.V_1_0.Test;

public class TestController : CleanArchitectureController
{
    public const string Base = "test/";

    public const string TestOptionInQueryObject = Base + "query/object";

    public const string TestOptionInQuery = Base + "query";

    /*
    public record TestOptionBinding(Option<string> String);

    [HttpGet(TestOptionInQueryObject)]
    public Task<ActionResult<ResponseDto<Option<string>, Nothing>>> GetPerson([FromQuery] TestOptionBinding value)
    {
        return Success(value.String);
    }
    */

    [HttpGet(TestOptionInQuery)]
    public Task<ActionResult<ResponseDto<Option<string>, Nothing>>> GetPerson([FromQuery] Option<string> value)
    {
        return Success(value);
    }
}
