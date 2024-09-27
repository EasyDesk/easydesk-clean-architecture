using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Single;
using EasyDesk.SampleApp.Application.V_1_0.Commands;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.ApiKeys;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Api;

public static class ApiKeyEndpoints
{
    public static HttpSingleRequestExecutor<Nothing> StoreApiKey(this HttpTestHelper http, string apiKey, AgentDto agent) =>
        http.Post<StoreApiKey, Nothing>(ApiKeysRoutes.StoreApiKey, new()
        {
            ApiKey = apiKey,
            Agent = agent,
        });

    public static HttpSingleRequestExecutor<Nothing> DeleteApiKey(this HttpTestHelper http, string apiKey) =>
        http.Delete<Nothing>(ApiKeysRoutes.DeleteApiKey.WithRouteParam(nameof(apiKey), apiKey));

    public static HttpSingleRequestExecutor<AgentDto> TestApiKey(this HttpTestHelper http) =>
        http.Get<AgentDto>(ApiKeysRoutes.TestApiKey);
}
