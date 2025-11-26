using EasyDesk.CleanArchitecture.Web.Controllers;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Options;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using Microsoft.AspNetCore.Mvc;

namespace EasyDesk.SampleApp.Web.Controllers.V_1_0.Test;

public class TestController : CleanArchitectureController
{
    public const string Base = "test";

    public const string TestOptionInQueryObjectRoute = Base + "/query/object";

    public const string TestOptionInQueryRoute = Base + "/query";

    public const string ThrowExceptionRoute = Base + "/exception";

    public const string TestPolymorphismRoute = Base + "/polymorphism";

    public const string NodaTimeTestRoute = Base + "/nodatime";

    public const string TestFixedMapStringObjectRoute = Base + "/fixedmap/stringobject";

    public const string TestFixedMapStringRecordRoute = Base + "/fixedmap/stringrecord";

    public const string TestFixedMapRecordRecordRoute = Base + "/fixedmap/recordrecord";

    [HttpGet(TestOptionInQueryRoute)]
    public Task<ActionResult<ResponseDto<Option<string>, Nothing>>> GetValueFromQuery([FromQuery] Option<string> value)
    {
        return Success(value);
    }

    [HttpGet(ThrowExceptionRoute)]
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

    public record TestRecord(string A, int B);

    [HttpPost(TestFixedMapStringObjectRoute)]
    public Task<ActionResult<ResponseDto<IFixedMap<string, object>, Nothing>>> TestFixedMapStringObject([FromBody] IFixedMap<string, object> body)
    {
        return Success(body);
    }

    [HttpPost(TestFixedMapStringRecordRoute)]
    public Task<ActionResult<ResponseDto<IFixedMap<string, TestRecord>, Nothing>>> TestFixedMapStringRecord([FromBody] IFixedMap<string, TestRecord> body)
    {
        return Success(body);
    }

    [HttpPost(TestFixedMapRecordRecordRoute)]
    public Task<ActionResult<ResponseDto<IFixedMap<TestRecord, TestRecord>, Nothing>>> TestFixedMapRecordRecord([FromBody] IFixedMap<TestRecord, TestRecord> body)
    {
        return Success(body);
    }
}
