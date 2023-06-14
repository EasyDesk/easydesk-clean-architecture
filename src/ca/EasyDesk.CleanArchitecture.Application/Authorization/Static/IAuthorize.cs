using EasyDesk.CleanArchitecture.Application.Authorization.Model;

namespace EasyDesk.CleanArchitecture.Application.Authorization.Static;

public interface IAuthorize
{
    bool IsAuthorized(AuthorizationInfo auth);
}
