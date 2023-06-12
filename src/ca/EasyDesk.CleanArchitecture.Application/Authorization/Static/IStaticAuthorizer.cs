using EasyDesk.CleanArchitecture.Application.ContextProvider;

namespace EasyDesk.CleanArchitecture.Application.Authorization.Static;

public interface IStaticAuthorizer
{
    Task<bool> IsAuthorized<T>(T request, UserInfo userInfo);
}
