using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Results;
using System.Collections.Immutable;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public static class Errors
{
    public static MultiError Multiple(Error primaryError, IEnumerable<Error> secondaryErrors) =>
        new(primaryError, secondaryErrors.ToEquatableList());

    public static InternalError Internal(Exception ex) => new(ex);

    public static NotFoundError NotFound() => new();

    public static ForbiddenError Forbidden(string? message = null) => new(message ?? "Not authorized");

    public static InvalidInputError InvalidInput(string propertyName, string errorCode, string errorMessage, IImmutableDictionary<string, object>? parameters = null) =>
        new(propertyName, errorCode, errorMessage, parameters ?? Map<string, object>());

    public static GenericError Generic(string code, string message, params object[] args) => GenericError.Create(code, message, args);
}
