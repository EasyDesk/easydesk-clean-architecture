using EasyDesk.CleanArchitecture.Application.Authorization;
using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.Authorization.Static;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using NSubstitute;
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
    private readonly IAuthorizationInfoProvider _authorizationInfoProvider;

    public DefaultStaticAuthorizerTests()
    {
        _authorizationInfoProvider = Substitute.For<IAuthorizationInfoProvider>();
        _authorizationInfoProvider.GetAuthorizationInfoForUser(_userInfo).Returns(new AuthorizationInfo(Set<Permission>()));
    }

    private DefaultStaticAuthorizer CreateAuthorizer<T>() => new(_authorizationInfoProvider);

    private async Task<bool> IsAuthorized<T>() where T : new() =>
        await CreateAuthorizer<T>().IsAuthorized(new T(), _userInfo);

    private void SetPermissions(params string[] permissions)
    {
        var permissionSet = permissions.Select(p => new Permission(p)).ToEquatableSet();
        _authorizationInfoProvider.GetAuthorizationInfoForUser(_userInfo).Returns(new AuthorizationInfo(permissionSet));
    }

    private async Task ShouldNotBeAuthorized<T>() where T : new()
    {
        var result = await IsAuthorized<T>();
        result.ShouldBe(false);
    }

    private async Task ShouldBeAuthorized<T>() where T : new()
    {
        var result = await IsAuthorized<T>();
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
        SetPermissions(A, C);
        await ShouldBeAuthorized<RequestWithRequirements>();
    }

    [Fact]
    public async Task ShouldNotAuthorizeTheUserIfTheyDoNotHaveCorrectPermissions()
    {
        SetPermissions(D);
        await ShouldNotBeAuthorized<RequestWithRequirements>();
    }

    [Fact]
    public async Task ShouldNotAuthorizeTheUserIfTheyDoHavePartiallyCorrectPermissions()
    {
        SetPermissions(A, B);
        await ShouldNotBeAuthorized<RequestWithRequirements>();
    }
}
