namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public record InvalidTenantIdError(string RawTenantId) : Error;
