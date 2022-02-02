using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Tools.Collections;
using System;
using System.Collections.Generic;

namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public static class Errors
{
    public static MultipleErrors Multiple(Error primaryError, IEnumerable<Error> secondaryErrors) =>
        new(primaryError, secondaryErrors.ToEquatableSet());

    public static Error Internal(Exception ex) => new InternalError(ex);

    public static Error NotFound() => new NotFoundError();

    public static Error UnknownUser() => new UnknownUserError();

    public static Error Forbidden(string message = null) => new ForbiddenError(message ?? "Not authorized");

    public static Error InvalidInput(string propertyName, string errorMessage) =>
        new InputValidationError(propertyName, errorMessage);

    public static Error FromDomain(DomainError domainError) => new DomainErrorWrapper(domainError);

    public static Error Generic(string message, params object[] args) => GenericError.Create(message, args);
}
