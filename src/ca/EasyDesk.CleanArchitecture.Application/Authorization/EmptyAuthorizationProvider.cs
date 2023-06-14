using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

internal class EmptyAuthorizationProvider : IAuthorizationProvider
{
    private readonly IContextProvider _contextProvider;

    public EmptyAuthorizationProvider(IContextProvider contextProvider)
    {
        _contextProvider = contextProvider;
    }

    public Task<Option<AuthorizationInfo>> GetAuthorizationInfo() => Task.FromResult(_contextProvider.GetIdentity().Map(u => new AuthorizationInfo(u, Set<Permission>())));
}
