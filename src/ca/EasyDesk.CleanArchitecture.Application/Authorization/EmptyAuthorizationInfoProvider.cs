using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

internal class EmptyAuthorizationInfoProvider : IAuthorizationInfoProvider
{
    private readonly IContextProvider _contextProvider;

    public EmptyAuthorizationInfoProvider(IContextProvider contextProvider)
    {
        _contextProvider = contextProvider;
    }

    public Task<Option<AuthorizationInfo>> GetAuthorizationInfo() => Task.FromResult(_contextProvider.GetUserInfo().Map(u => new AuthorizationInfo(u, Set<Permission>())));
}
