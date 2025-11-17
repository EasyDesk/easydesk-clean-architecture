using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Single;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.Test;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Api;

public static class TestEndpoints
{
    public static HttpSingleRequestExecutor<NodaTimeTestDto> TestNodaTimeSerialization(this HttpTestHelper http, NodaTimeTestDto body) =>
        http.Post<NodaTimeTestDto, NodaTimeTestDto>(TestController.NodaTimeTestRoute, body);
}
