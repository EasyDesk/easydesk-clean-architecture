using EasyDesk.CleanArchitecture.Application.ErrorManagement;

namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public record TenantNotFoundError(TenantId TenantId) : ApplicationError
{
    public override string GetDetail() => "The provided tenant doesn't exist";
}
