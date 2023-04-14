using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using NSubstitute;
using Shouldly;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Authorization;

public class RoleBasedAuthorizerTests
{
    private const string A = "A";
    private const string B = "B";
    private const string C = "C";
    private const string D = "D";

    private record RequestWithNoRequirements;

    [RequireAnyOf(A, B)]
    [RequireAnyOf(C)]
    private record RequestWithRequirements;

    private readonly UserInfo _userInfo = UserInfo.Create(UserId.New("user"));
    private readonly IPermissionsProvider _permissionsProvider;

    public RoleBasedAuthorizerTests()
    {
        _permissionsProvider = Substitute.For<IPermissionsProvider>();
        _permissionsProvider.GetPermissionsForUser(_userInfo).Returns(Set<Permission>());
    }

    private RoleBasedAuthorizer CreateAuthorizer<T>() => new(_permissionsProvider);

    private async Task<bool> IsAuthorized<T>() where T : notnull, new() =>
        await CreateAuthorizer<T>().IsAuthorized(new T(), _userInfo);

    private void SetPermissions(params string[] permissions)
    {
        var permissionSet = permissions.Select(p => new Permission(p)).ToEquatableSet();
        _permissionsProvider.GetPermissionsForUser(_userInfo).Returns(permissionSet);
    }

    private async Task ShouldNotBeAuthorized<T>() where T : notnull, new()
    {
        var result = await IsAuthorized<T>();
        result.ShouldBe(false);
    }

    private async Task ShouldBeAuthorized<T>() where T : notnull, new()
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
