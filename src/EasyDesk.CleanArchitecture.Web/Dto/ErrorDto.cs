using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Web.Dto;

public record ErrorDto(string Code, string Detail, object Meta)
{
    public static IEnumerable<ErrorDto> CreateErrorDtoList(Error error) => error switch
    {
        MultipleErrors(var errors) => errors.Select(FromError),
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
        DomainErrorWrapper(var domainError) => new(
            Code: domainError.GetType().Name,
            Detail: $"Domain Error: {domainError.GetType().Name}",
            Meta: domainError),
        InputValidationError(var propertyName, var errorMessage) => new(
            Code: "InputValidationError",
            Detail: errorMessage,
            Meta: new { PropertyName = propertyName }),
        _ => throw new ArgumentException("Can't convert to single error", nameof(error))
    };
}
