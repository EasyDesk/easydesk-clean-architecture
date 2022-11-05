using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.Tools.Collections;
using System.Collections.Immutable;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;

internal class RoleBasedAuthorizer : IAuthorizer
{
    private readonly IPermissionsProvider _permissionsProvider;

    public RoleBasedAuthorizer(IPermissionsProvider permissionsProvider)
    {
        _permissionsProvider = permissionsProvider;
    }

    public async Task<bool> IsAuthorized<T>(T request, UserInfo userInfo)
    {
        var requirementAttributes = request.GetType().GetCustomAttributes<RequireAnyOfAttribute>();
        if (requirementAttributes.IsEmpty())
        {
            return true;
        }

        var userPermissions = await _permissionsProvider.GetPermissionsForUser(userInfo);
        return requirementAttributes.All(a => HasCorrectPermissions(a, userPermissions));
    }

    private bool HasCorrectPermissions(RequireAnyOfAttribute attribute, IImmutableSet<Permission> userPermissions) =>
        userPermissions.Overlaps(attribute.Permissions);
}
