using EasyDesk.CleanArchitecture.Application.Authorization;
using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using NSubstitute;
using Shouldly;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static EasyDesk.Tools.Collections.ImmutableCollections;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Mediator.Behaviors;

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

    private readonly UserInfo _userInfo = new("user");
    private readonly IPermissionsProvider _permissionsProvider;

    public RoleBasedAuthorizerTests()
    {
        _permissionsProvider = Substitute.For<IPermissionsProvider>();
        _permissionsProvider.GetPermissionsForUser(_userInfo).Returns(Set<Permission>());
    }

    private RoleBasedAuthorizer<T> CreateAuthorizer<T>() =>
        new(_permissionsProvider);

    private async Task<bool> IsAuthorized<T>() where T : new() =>
        await CreateAuthorizer<T>().IsAuthorized(new T(), _userInfo);

    private void SetPermissions(params string[] permissions)
    {
        var permissionSet = permissions.Select(p => new Permission(p)).ToEquatableSet();
        _permissionsProvider.GetPermissionsForUser(_userInfo).Returns(permissionSet);
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
