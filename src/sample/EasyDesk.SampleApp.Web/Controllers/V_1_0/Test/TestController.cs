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

    public const string TestGuidInQueryRoute = Base + "/query/guid";

    public const string TestGuidInBodyRoute = Base + "/body/guid";

    public const string TestGuidInRouteRoute = Base + "/route/{value}";

    public const string ThrowExceptionRoute = Base + "/exception";

    public const string TestPolymorphismRoute = Base + "/polymorphism";

    public const string NodaTimeTestRoute = Base + "/nodatime";

    public const string TestFixedMapStringObjectRoute = Base + "/fixedmap/stringobject";

    public const string TestFixedMapStringRecordRoute = Base + "/fixedmap/stringrecord";

    public const string TestFixedMapRecordRecordRoute = Base + "/fixedmap/recordrecord";

    public const string TestFixedMapEnumRecordRoute = Base + "/fixedmap/enumrecord";

    public const string TestDictionaryEnumRecordRoute = Base + "/dictionary/enumrecord";

    public const string TestDictionaryStringRecordRoute = Base + "/dictionary/stringrecord";

    public const string TestDictionaryStringObjectRoute = Base + "/dictionary/stringobject";

    public const string TestOptionStringRoute = Base + "/option/string";

    public const string TestOptionNumberRoute = Base + "/option/number";

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

    public enum EnumType
    {
        A,
        B,
        C,
    }

    [HttpPost(TestFixedMapEnumRecordRoute)]
    public Task<ActionResult<ResponseDto<IFixedMap<EnumType, TestRecord>, Nothing>>> TestFixedMapEnumRecord([FromBody] IFixedMap<EnumType, TestRecord> body)
    {
        return Success(body);
    }

    [HttpPost(TestDictionaryEnumRecordRoute)]
    public Task<ActionResult<ResponseDto<IDictionary<EnumType, TestRecord>, Nothing>>> TestDictionaryEnumRecord([FromBody] IDictionary<EnumType, TestRecord> body)
    {
        return Success(body);
    }

    [HttpPost(TestDictionaryStringRecordRoute)]
    public Task<ActionResult<ResponseDto<IDictionary<string, TestRecord>, Nothing>>> TestDictionaryStringRecord([FromBody] IDictionary<string, TestRecord> body)
    {
        return Success(body);
    }

    [HttpPost(TestDictionaryStringObjectRoute)]
    public Task<ActionResult<ResponseDto<IDictionary<string, object?>, Nothing>>> TestDictionaryStringObject([FromBody] IDictionary<string, object?> body)
    {
        return Success(body);
    }

    [HttpGet(TestGuidInQueryRoute)]
    public Task<ActionResult<ResponseDto<Option<Guid>, Nothing>>> TestGuidInQuery([FromQuery] Option<Guid> value)
    {
        return Success(value);
    }

    public record GuidInBody(Option<Guid> Value);

    [HttpPost(TestGuidInBodyRoute)]
    public Task<ActionResult<ResponseDto<Option<Guid>, Nothing>>> TestGuidInBody([FromBody] GuidInBody value)
    {
        return Success(value.Value);
    }

    [HttpGet(TestGuidInRouteRoute)]
    public Task<ActionResult<ResponseDto<Option<Guid>, Nothing>>> TestGuidInRoute([FromRoute] Option<Guid> value)
    {
        return Success(value);
    }

    [HttpGet(TestOptionStringRoute)]
    public Task<ActionResult<ResponseDto<Option<string>, Nothing>>> TestOptionString()
    {
        return Success(Some("hello"));
    }

    [HttpGet(TestOptionNumberRoute)]
    public Task<ActionResult<ResponseDto<Option<double>, Nothing>>> TestOptionNumber()
    {
        return Success(Some(42.0));
    }
}
