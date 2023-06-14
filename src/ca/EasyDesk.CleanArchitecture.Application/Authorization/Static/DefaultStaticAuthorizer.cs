using EasyDesk.CleanArchitecture.Application.Authorization.Model;

namespace EasyDesk.CleanArchitecture.Application.Authorization.Static;

public class DefaultStaticAuthorizer<T> : IStaticAuthorizer<T>
{
    public bool IsAuthorized(T request, AuthorizationInfo authorizationInfo)
    {
        return request is not IAuthorize authorize || authorize.IsAuthorized(authorizationInfo);
    }
}
