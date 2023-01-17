using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Single;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.Admins;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Api;

public static class AdminEndpoints
{
    public static HttpSingleRequestExecutor<Nothing> AddAdmin(this HttpTestHelper http) =>
        http.Post<Nothing, Nothing>(AdminRoutes.AddAdmin, Nothing.Value);
}
