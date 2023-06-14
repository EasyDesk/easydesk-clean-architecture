using EasyDesk.CleanArchitecture.Application.Authorization.Model;

namespace EasyDesk.CleanArchitecture.Application.Authorization.Static;

public interface IStaticAuthorizationRequirement
{
    bool IsSatisfied(AuthorizationInfo authorizationInfo);
}
