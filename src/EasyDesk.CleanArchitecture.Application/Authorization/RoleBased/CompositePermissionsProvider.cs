using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using static EasyDesk.Tools.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;

public class CompositePermissionsProvider : IPermissionsProvider
{
    private readonly IEnumerable<IPermissionsProvider> _providers;

    public CompositePermissionsProvider(IEnumerable<IPermissionsProvider> providers)
    {
        _providers = providers;
    }

    public async Task<IImmutableSet<Permission>> GetPermissionsForUser(UserInfo userDescription)
    {
        var allPermissions = Set<Permission>();
        foreach (var provider in _providers)
        {
            var permissions = await provider.GetPermissionsForUser(userDescription);
            allPermissions = allPermissions.Union(permissions);
        }
        return allPermissions;
    }
}
