using EasyDesk.CleanArchitecture.Application.Authorization.Model;

namespace EasyDesk.CleanArchitecture.Application.Authorization.Static;

public interface IStaticAuthorizer<T>
{
    bool IsAuthorized(T request, AuthorizationInfo authorizationInfo);
}
