using EasyDesk.CleanArchitecture.Application.Authorization;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Domain.Metamodel;

namespace EasyDesk.CleanArchitecture.Web.Dto;

public record ErrorDto(string Code, string Detail, object Meta)
{
    public static IEnumerable<ErrorDto> CreateErrorDtoList(Error error) => error switch
    {
        MultiError(var errors) => errors.Select(FromError),
        _ => Some(FromError(error))
    };

    public static ErrorDto FromError(Error error) => error switch
    {
        NotFoundError => new(
            Code: "NotFound",
            Detail: "Unable to fine the requested resource",
            Meta: Nothing.Value),
        UnknownUserError => new(
            Code: "UnknownUser",
            Detail: "Missing authentication information on the given request",
            Meta: Nothing.Value),
        ForbiddenError(var reason) => new(
            Code: "Forbidden",
            Detail: $"Forbidden: {reason}",
            Meta: Nothing.Value),
        InvalidTenantIdError(string) => new(
            Code: "InvalidTenant",
            Detail: "The provided tenant is invalid",
            Meta: Nothing.Value),
        TenantNotFoundError(var tenantId) => new(
            Code: "TenantNotFound",
            Detail: "The provided tenant doesn't exist",
            Meta: new { TenantId = tenantId }),
        MissingTenantError => new(
            Code: "MissingTenant",
            Detail: "Missing tenant information on the given request",
            Meta: Nothing.Value),
        MultitenancyNotSupportedError => new(
            Code: "MultitenancyNotSupported",
            Detail: "The request did provide a tenant but multitenancy isn't supported",
            Meta: Nothing.Value),
        InputValidationError(var propertyName, var errorMessage) => new(
            Code: "InvalidInput",
            Detail: errorMessage,
            Meta: new { PropertyName = propertyName }),
        DomainError => new(
            Code: error.GetType().Name,
            Detail: $"Domain Error: {error.GetType().Name}",
            Meta: error),
        GenericError(var message, var parameters) => new(
            Code: "Generic",
            Detail: message,
            Meta: parameters),
        _ => new(
            Code: "Internal",
            Detail: "Unknown internal error occurred",
            Meta: Nothing.Value),
    };
}
