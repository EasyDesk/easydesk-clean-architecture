using EasyDesk.CleanArchitecture.Application.Authorization;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Application.Mediator.Behaviors;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.Tools;
using NSubstitute;
using Shouldly;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static EasyDesk.CleanArchitecture.Application.Responses.ResponseImports;
using static EasyDesk.Tools.Collections.ImmutableCollections;
using static EasyDesk.Tools.Options.OptionImports;
using Next = MediatR.RequestHandlerDelegate<EasyDesk.CleanArchitecture.Application.Responses.Response<EasyDesk.Tools.Nothing>>;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Mediator.Behaviors;

public class AuthorizationBehaviorTests
{
    private const string A = "A";
    private const string B = "B";
    private const string C = "C";
    private const string D = "D";

    [AllowUnknownUser]
    private record RequestWithUnknownUserAllowed : RequestBase<Nothing>;

    private record RequestWithNoRequirements : RequestBase<Nothing>;

    [RequireAnyOf(A, B)]
    [RequireAnyOf(C)]
    private record RequestWithRequirements : RequestBase<Nothing>;

    private readonly UserInfo _userInfo = new("user");
    private readonly IPermissionsProvider _permissionsProvider;
    private readonly IUserInfoProvider _userInfoProvider;
    private readonly Next _next;

    public AuthorizationBehaviorTests()
    {
        _permissionsProvider = Substitute.For<IPermissionsProvider>();
        _permissionsProvider.GetPermissionsForUser(_userInfo).Returns(Set<Permission>());

        _userInfoProvider = Substitute.For<IUserInfoProvider>();
        _userInfoProvider.GetUserInfo().Returns(None);

        _next = Substitute.For<Next>();
        _next().Returns(Ok);
    }

    private AuthorizationBehavior<T, Nothing> CreateBehavior<T>() where T : RequestBase<Nothing> =>
        new(_permissionsProvider, _userInfoProvider);

    private async Task<Response<Nothing>> Handle<T>() where T : RequestBase<Nothing>, new() =>
        await CreateBehavior<T>().Handle(new T(), default, _next);

    private void Authenticate() => _userInfoProvider.GetUserInfo().Returns(_userInfo);

    private void SetPermissions(params string[] permissions)
    {
        var permissionSet = permissions.Select(p => new Permission(p)).ToEquatableSet();
        _permissionsProvider.GetPermissionsForUser(_userInfo).Returns(permissionSet);
    }

    private async Task ShouldNotBeAuthorized<T>(Error error) where T : RequestBase<Nothing>, new()
    {
        var result = await Handle<T>();
        result.ShouldBe(error);
        await _next.DidNotReceive()();
    }

    private async Task ShouldBeAuthorized<T>() where T : RequestBase<Nothing>, new()
    {
        var result = await Handle<T>();
        result.ShouldBe(Ok);
        await _next.Received(1)();
    }

    [Fact]
    public async Task ShouldBeAuthorized_IfRequestAllowsUnknownUserAndUserIsNotAuthenticated()
    {
        await ShouldBeAuthorized<RequestWithUnknownUserAllowed>();
    }

    [Fact]
    public async Task ShouldBeAuthorized_IfRequestAllowsUnknownUserAndUserIsAuthenticated()
    {
        Authenticate();
        await ShouldBeAuthorized<RequestWithUnknownUserAllowed>();
    }

    [Fact]
    public async Task ShouldBeAuthorized_IfRequestHasNoRequirementsAndUserIsAuthenticated()
    {
        Authenticate();
        await ShouldBeAuthorized<RequestWithNoRequirements>();
    }

    [Fact]
    public async Task ShouldNotBeAuthorized_ReturningUnknownUser_IfTheRequestHasNoRequirementsButUserIsNotAuthenticated()
    {
        await ShouldNotBeAuthorized<RequestWithNoRequirements>(Errors.UnknownUser());
    }

    [Fact]
    public async Task ShouldBeAuthorized_IfUserHasTheCorrectPermissions()
    {
        Authenticate();
        SetPermissions(A, C);
        await ShouldBeAuthorized<RequestWithRequirements>();
    }

    [Fact]
    public async Task ShouldNotBeAuthorized_ReturningForbidden_IfUserDoesNotHaveCorrectPermissions()
    {
        Authenticate();
        SetPermissions(D);
        await ShouldNotBeAuthorized<RequestWithRequirements>(Errors.Forbidden());
    }

    [Fact]
    public async Task ShouldNotBeAuthorized_ReturningForbidden_IfUserHasPartiallyCorrectPermissions()
    {
        Authenticate();
        SetPermissions(A, B);
        await ShouldNotBeAuthorized<RequestWithRequirements>(Errors.Forbidden());
    }
}
