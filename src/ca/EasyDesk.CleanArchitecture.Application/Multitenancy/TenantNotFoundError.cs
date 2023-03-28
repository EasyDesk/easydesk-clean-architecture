namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public record TenantNotFoundError(TenantId TenantId) : Error;
