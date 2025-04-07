using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Extensions;
using EasyDesk.CleanArchitecture.Web.Authentication.ApiKey;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using Microsoft.Net.Http.Headers;
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
        await Session.Http
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

        await Session.Http
            .StoreApiKey(TestApiKey, updatedAgent)
            .Send()
            .EnsureSuccess();
    }

    [Fact]
    public async Task DeletingApiKey_ShouldSucceed()
    {
        await Session.Http
            .DeleteApiKey(TestApiKey)
            .Send()
            .EnsureSuccess();
    }

    [Fact]
    public async Task TestingApiKey_ShouldReturnAgent_IfAuthenticationSucceedes()
    {
        await Session.Http
            .StoreApiKey(TestApiKey, _testAgent)
            .Send()
            .EnsureSuccess();

        var result = await Session.Http
            .TestApiKey()
            .WithQuery(ApiKeyOptions.ApiKeyDefaultQueryParameter, TestApiKey)
            .Send()
            .AsData();

        result.ShouldBe(_testAgent);
    }

    [Fact]
    public async Task TestingApiKey_ShouldReturnAgent_IfAuthenticationSucceedes_UsingHeader()
    {
        await Session.Http
            .StoreApiKey(TestApiKey, _testAgent)
            .Send()
            .EnsureSuccess();

        var result = await Session.Http
            .TestApiKey()
            .Headers(h => h.Add(HeaderNames.Authorization, $"{ApiKeyOptions.ApiKeyDefaultScheme} {TestApiKey}"))
            .Send()
            .AsData();

        result.ShouldBe(_testAgent);
    }

    [Fact]
    public async Task TestingApiKey_ShouldFail_IfAuthenticationFails()
    {
        await Session.Http
            .StoreApiKey(TestApiKey, _testAgent)
            .Send()
            .EnsureSuccess();

        await Session.Http
            .TestApiKey()
            .WithQuery(ApiKeyOptions.ApiKeyDefaultQueryParameter, "some_invalid_api_key")
            .Send()
            .Verify();
    }

    [Fact]
    public async Task TestingApiKey_ShouldFail_AfterDeletingApiKey()
    {
        await Session.Http
            .StoreApiKey(TestApiKey, _testAgent)
            .Send()
            .EnsureSuccess();

        await Session.Http
            .DeleteApiKey(TestApiKey)
            .Send()
            .EnsureSuccess();

        await Session.Http
            .TestApiKey()
            .WithQuery(ApiKeyOptions.ApiKeyDefaultQueryParameter, TestApiKey)
            .Send()
            .Verify();
    }
}
