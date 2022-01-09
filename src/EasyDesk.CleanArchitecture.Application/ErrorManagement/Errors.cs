using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using static EasyDesk.Tools.Collections.ImmutableCollections;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public static class Errors
{
    public static MultipleErrors Multiple(IEnumerable<Error> errors) => new MultipleErrors(errors.ToEquatableList());

    public static Error Internal(Exception ex) => new InternalError(ex);

    public static Error Forbidden(string message = null) => new ForbiddenError(message ?? "Not authorized");

    public static Error InvalidInput(string propertyName, string errorMessage) =>
        new InputValidationError(propertyName, errorMessage);

    public static Error FromDomain(DomainError domainError) => new DomainErrorWrapper(domainError);

    public static Error FromDomain(IEnumerable<DomainError> domainErrors) => Multiple(domainErrors.Select(FromDomain));

    public static Error Generic(string message, params object[] args) => GenericError.Create(message, args);
}
