namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public record TenantNotFoundError(string TenantId) : Error;
