using EasyDesk.CleanArchitecture.Application.ErrorManagement;

namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public record MultitenancyNotSupportedError : ApplicationError
{
    public override string GetDetail() => "The request did provide a tenant but multitenancy isn't supported";
}
