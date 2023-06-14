using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using System.Collections.Immutable;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

internal class EmptyAuthorizationProvider : IIdentityPermissionsProvider
{
    public Task<IImmutableSet<Permission>> GetPermissionsForIdentity(Identity identity) =>
        Task.FromResult(Set<Permission>());
}
