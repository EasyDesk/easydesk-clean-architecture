using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.Authorization.Static;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using Shouldly;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Authorization;

public class DefaultStaticAuthorizerTests
{
    private const string A = "A";
    private const string B = "B";
    private const string C = "C";
    private const string D = "D";

    private record RequestWithNoRequirements;

    [RequireAnyOf(A, B)]
    [RequireAnyOf(C)]
    private record RequestWithRequirements;

    private readonly UserInfo _userInfo = new(UserId.New("user"));

    private readonly DefaultStaticAuthorizer _sut = new();

    private async Task<bool> IsAuthorized<T>(params string[] permissions) where T : new() =>
        await _sut.IsAuthorized(new T(), new AuthorizationInfo(_userInfo, permissions.Select(p => new Permission(p)).ToEquatableSet()));

    private async Task ShouldNotBeAuthorized<T>(params string[] permissions) where T : new()
    {
        var result = await IsAuthorized<T>(permissions);
        result.ShouldBe(false);
    }

    private async Task ShouldBeAuthorized<T>(params string[] permissions) where T : new()
    {
        var result = await IsAuthorized<T>(permissions);
        result.ShouldBe(true);
    }

    [Fact]
    public async Task ShouldAuthorizeTheUserIfTheRequestHasNoRequirements()
    {
        await ShouldBeAuthorized<RequestWithNoRequirements>();
    }

    [Fact]
    public async Task ShouldAuthorizeTheUserIfTheyHaveTheCorrectPermissions()
    {
        await ShouldBeAuthorized<RequestWithRequirements>(A, C);
    }

    [Fact]
    public async Task ShouldNotAuthorizeTheUserIfTheyDoNotHaveCorrectPermissions()
    {
        await ShouldNotBeAuthorized<RequestWithRequirements>(D);
    }

    [Fact]
    public async Task ShouldNotAuthorizeTheUserIfTheyDoHavePartiallyCorrectPermissions()
    {
        await ShouldNotBeAuthorized<RequestWithRequirements>(A, B);
    }
}
