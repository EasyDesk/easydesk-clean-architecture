using EasyDesk.CleanArchitecture.Application.Authorization.Model;

namespace EasyDesk.CleanArchitecture.Application.Authorization.Static;

public interface IStaticAuthorizer
{
    Task<bool> IsAuthorized<T>(T request, AuthorizationInfo authorizationInfo);
}
