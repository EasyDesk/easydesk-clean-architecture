using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.ContextProvider;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

public interface IAuthorizationInfoProvider
{
    Task<AuthorizationInfo> GetAuthorizationInfoForUser(UserInfo userInfo);
}
