using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Single;
using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.Test;
using static EasyDesk.SampleApp.Web.Controllers.V_1_0.Test.TestController;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Api;

public static class TestEndpoints
{
    public static HttpSingleRequestExecutor<NodaTimeTestDto> TestNodaTimeSerialization(this HttpTestHelper http, NodaTimeTestDto body) =>
        http.Post<NodaTimeTestDto, NodaTimeTestDto>(TestController.NodaTimeTestRoute, body);

    public static HttpSingleRequestExecutor<IFixedMap<string, object>> TestFixedMapStringObject(this HttpTestHelper http, IFixedMap<string, object> body) =>
        http.Post<IFixedMap<string, object>, IFixedMap<string, object>>(TestFixedMapStringObjectRoute, body);

    public static HttpSingleRequestExecutor<IFixedMap<string, TestRecord>> TestFixedMapStringRecord(this HttpTestHelper http, IFixedMap<string, TestRecord> body) =>
        http.Post<IFixedMap<string, TestRecord>, IFixedMap<string, TestRecord>>(TestFixedMapStringRecordRoute, body);

    public static HttpSingleRequestExecutor<IFixedMap<TestRecord, TestRecord>> TestFixedMapRecordRecord(this HttpTestHelper http, IFixedMap<TestRecord, TestRecord> body) =>
        http.Post<IFixedMap<TestRecord, TestRecord>, IFixedMap<TestRecord, TestRecord>>(TestFixedMapRecordRecordRoute, body);
}
