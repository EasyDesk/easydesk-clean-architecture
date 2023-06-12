using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.ContextProvider;

namespace EasyDesk.CleanArchitecture.Application.Authorization.Static;

public interface IStaticAuthorizationRequirement
{
    bool IsSatisfied(UserInfo userInfo, AuthorizationInfo authorizationInfo);
}
