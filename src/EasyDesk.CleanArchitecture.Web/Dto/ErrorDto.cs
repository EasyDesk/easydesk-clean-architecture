using EasyDesk.CleanArchitecture.Application.Authorization;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;

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
        InternalError => new(
            Code: "Internal",
            Detail: "Unknown internal error occurred",
            Meta: Nothing.Value),
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
        GenericError(var message, var parameters) => new(
            Code: "Generic",
            Detail: message,
            Meta: parameters),
        InputValidationError(var propertyName, var errorMessage) => new(
            Code: "InputValidationError",
            Detail: errorMessage,
            Meta: new { PropertyName = propertyName }),
        _ => new(
            Code: error.GetType().Name,
            Detail: $"Domain Error: {error.GetType().Name}",
            Meta: error)
    };
}
