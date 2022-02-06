using EasyDesk.Tools.Collections;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;

public class RoleBasedAuthorizer<T> : IAuthorizer<T>
{
    private readonly IPermissionsProvider _permissionsProvider;

    public RoleBasedAuthorizer(IPermissionsProvider permissionsProvider)
    {
        _permissionsProvider = permissionsProvider;
    }

    public async Task<bool> IsAuthorized(T request, UserInfo userInfo)
    {
        var requirementAttributes = typeof(T).GetCustomAttributes<RequireAnyOfAttribute>();
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
