using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

internal class EmptyAuthorizationInfoProvider : IAuthorizationInfoProvider
{
    public Task<AuthorizationInfo> GetAuthorizationInfoForUser(UserInfo userInfo) =>
        Task.FromResult(new AuthorizationInfo(Set<Permission>()));
}
