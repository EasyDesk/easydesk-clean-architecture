using EasyDesk.CleanArchitecture.Application.ErrorManagement;

namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public record MissingTenantError : ApplicationError
{
    public override string GetDetail() => "Missing tenant information on the given request";
}
