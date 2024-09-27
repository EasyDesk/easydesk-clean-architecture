using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Extensions;
using EasyDesk.CleanArchitecture.Web.Authentication.ApiKey;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using Shouldly;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.IntegrationTests.ApiKeys;

public class ApiKeysTests : SampleIntegrationTest
{
    private const string TestApiKey = "some_long_api_key";
    private readonly AgentDto _testAgent = AgentDto.MapFrom(Agent.Construct(x =>
    {
        x.AddIdentity(new("realm-A"), new("id-A"))
            .AddAttribute("a", "aa")
            .AddAttribute("a", "aaa")
            .AddAttribute("b", "bb");

        x.AddIdentity(new("realm-B"), new("id-B"))
            .AddAttribute("a", "aa")
            .AddAttribute("b", "aa");
    }));

    public ApiKeysTests(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task StoringApiKey_ShouldSucceed()
    {
        await Http
            .StoreApiKey(TestApiKey, _testAgent)
            .Send()
            .EnsureSuccess();
    }

    [Fact]
    public async Task StoringApiKey_ShouldSucceed_WithSameApiKeyAndDifferentAgent()
    {
        var updatedAgent = _testAgent with
        {
            Identities = _testAgent.Identities.Add(new IdentityDto(
                "id-C",
                "realm-C",
                Map(("a", Set("eh"))))),
        };

        await Http
            .StoreApiKey(TestApiKey, updatedAgent)
            .Send()
            .EnsureSuccess();
    }

    [Fact]
    public async Task TestingApiKey_ShouldReturnAgent_IfAuthenticationSucceedes()
    {
        await Http
            .StoreApiKey(TestApiKey, _testAgent)
            .Send()
            .EnsureSuccess();

        var result = await Http
            .TestApiKey()
            .WithQuery(ApiKeyOptions.ApiKeyDefaultQueryParameter, TestApiKey)
            .Send()
            .AsData();

        result.ShouldBe(_testAgent);
    }



    [Fact]
    public async Task TestingApiKey_ShouldFail_IfAuthenticationFails()
    {
        await Http
            .StoreApiKey(TestApiKey, _testAgent)
            .Send()
            .EnsureSuccess();

        await Http
            .TestApiKey()
            .WithQuery(ApiKeyOptions.ApiKeyDefaultQueryParameter, "some_invalid_api_key")
            .Send()
            .Verify();
    }
}
