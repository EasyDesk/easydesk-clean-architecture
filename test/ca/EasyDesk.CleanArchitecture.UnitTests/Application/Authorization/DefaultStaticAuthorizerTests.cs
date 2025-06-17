using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.Authorization.Static;
using Shouldly;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Authorization;

public class DefaultStaticAuthorizerTests
{
    private enum TestPermissions
    {
        A,
        B,
        C,
        D,
    }

    private record RequestWithNoRequirements;

    private record RequestWithRequirements : IAuthorize
    {
        public bool IsAuthorized(AuthorizationInfo auth)
        {
            return auth.HasAnyPermissionAmong(TestPermissions.A, TestPermissions.B)
                && auth.HasPermission(TestPermissions.C);
        }
    }

    private readonly Agent _agent = Agent.FromSingleIdentity(Realm.Default, new IdentityId("identity"));

    private bool IsAuthorized<T>(params TestPermissions[] permissions) where T : new()
    {
        return new DefaultStaticAuthorizer<T>()
            .IsAuthorized(new T(), new AuthorizationInfo(_agent, permissions.Select(p => (Permission)p).ToFixedSet()));
    }

    [Fact]
    public void ShouldAuthorizeTheIdentity_IfTheRequestHasNoRequirements()
    {
        IsAuthorized<RequestWithNoRequirements>()
            .ShouldBeTrue();
    }

    [Fact]
    public void ShouldAuthorizeTheIdentity_IfTheyHaveTheCorrectPermissions()
    {
        IsAuthorized<RequestWithRequirements>(TestPermissions.A, TestPermissions.C)
            .ShouldBeTrue();
    }

    [Fact]
    public void ShouldNotAuthorizeTheIdentity_IfTheyDoNotHaveCorrectPermissions()
    {
        IsAuthorized<RequestWithRequirements>(TestPermissions.D)
            .ShouldBeFalse();
    }

    [Fact]
    public void ShouldNotAuthorizeTheIdentity_IfTheyDoHavePartiallyCorrectPermissions()
    {
        IsAuthorized<RequestWithRequirements>(TestPermissions.A, TestPermissions.B)
            .ShouldBeFalse();
    }
}
