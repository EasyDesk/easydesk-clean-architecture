using System.Collections.Immutable;

namespace EasyDesk.Commons;

public record MultiError : Error
{
    public MultiError(Error primaryError, IImmutableSet<Error> secondaryErrors)
    {
        if (primaryError is MultiError)
        {
            throw new ArgumentException("Primary error cannot be an instance of MultipleError", nameof(primaryError));
        }
        if (secondaryErrors.Any(e => e is MultiError))
        {
            throw new ArgumentException("Secondary errors cannot contain an instance of MultipleError", nameof(secondaryErrors));
        }
        PrimaryError = primaryError;
        SecondaryErrors = secondaryErrors;
    }

    public Error PrimaryError { get; init; }

    public IImmutableSet<Error> SecondaryErrors { get; init; }

    public void Deconstruct(out IImmutableSet<Error> errors) => errors = SecondaryErrors.Add(PrimaryError);

    public void Deconstruct(out Error primaryError, out IImmutableSet<Error> secondaryErrors)
    {
        primaryError = PrimaryError;
        secondaryErrors = SecondaryErrors;
    }
}
