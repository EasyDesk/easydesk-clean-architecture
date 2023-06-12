using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.Commons.Collections;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.Authorization.Static;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class RequireAnyOfAttribute : Attribute, IStaticAuthorizationRequirement
{
    private readonly IImmutableSet<Permission> _permissions;

    public RequireAnyOfAttribute(params object[] permissions)
    {
        _permissions = permissions.Select(x => new Permission(x.ToString())).ToEquatableSet();
    }

    public bool IsSatisfied(UserInfo userInfo, AuthorizationInfo authorizationInfo)
    {
        return _permissions.Overlaps(authorizationInfo.Permissions);
    }
}
