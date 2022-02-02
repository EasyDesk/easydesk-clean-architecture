using System;
using System.Collections.Immutable;
using System.Linq;

namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public record MultipleErrors : Error
{
    public MultipleErrors(Error primaryError, IImmutableSet<Error> secondaryErrors)
    {
        if (primaryError is MultipleErrors)
        {
            throw new ArgumentException("Primary error cannot be an instance of MultipleError", nameof(primaryError));
        }
        if (secondaryErrors.Any(e => e is MultipleErrors))
        {
            throw new ArgumentException("Secondary errors cannot contain an instance of MultipleError", nameof(secondaryErrors));
        }
        PrimaryError = primaryError;
        SecondaryErrors = secondaryErrors;
    }

    public void Deconstruct(out IImmutableSet<Error> errors) => errors = SecondaryErrors.Add(PrimaryError);

    public void Deconstruct(out Error primaryError, out IImmutableSet<Error> secondaryErrors)
    {
        primaryError = PrimaryError;
        secondaryErrors = SecondaryErrors;
    }

    public Error PrimaryError { get; init; }

    public IImmutableSet<Error> SecondaryErrors { get; init; }
}
