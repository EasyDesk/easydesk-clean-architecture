using EasyDesk.Commons.Collections;

namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public static class Errors
{
    public static MultiError Multiple(Error primaryError, IEnumerable<Error> secondaryErrors) =>
        new(primaryError, secondaryErrors.ToEquatableSet());

    public static InternalError Internal(Exception ex) => new(ex);

    public static NotFoundError NotFound() => new();

    public static ForbiddenError Forbidden(string? message = null) => new(message ?? "Not authorized");

    public static InputValidationError InvalidInput(string propertyName, string errorMessage) =>
        new(propertyName, errorMessage);

    public static GenericError Generic(string message, params object[] args) => GenericError.Create(message, args);
}