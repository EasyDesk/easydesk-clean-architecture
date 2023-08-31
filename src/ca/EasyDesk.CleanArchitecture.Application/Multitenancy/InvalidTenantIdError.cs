using EasyDesk.CleanArchitecture.Application.ErrorManagement;

namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public record InvalidTenantIdError(string RawTenantId) : ApplicationError
{
    public override string GetDetail() => "The provided tenant is invalid";
}
