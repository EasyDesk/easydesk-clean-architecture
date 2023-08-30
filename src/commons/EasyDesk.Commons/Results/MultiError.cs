using System.Collections.Immutable;

namespace EasyDesk.Commons.Results;

public record MultiError : Error
{
    public MultiError(Error primaryError, IImmutableList<Error> secondaryErrors)
    {
        if (primaryError is MultiError)
        {
            throw new ArgumentException($"Primary error cannot be an instance of {nameof(MultiError)}", nameof(primaryError));
        }
        if (secondaryErrors.Any(e => e is MultiError))
        {
            throw new ArgumentException($"Secondary errors cannot contain an instance of {nameof(MultiError)}", nameof(secondaryErrors));
        }
        PrimaryError = primaryError;
        SecondaryErrors = secondaryErrors;
    }

    public Error PrimaryError { get; init; }

    public IImmutableList<Error> SecondaryErrors { get; init; }

    public void Deconstruct(out IImmutableList<Error> errors) => errors = SecondaryErrors.Add(PrimaryError);

    public void Deconstruct(out Error primaryError, out IImmutableList<Error> secondaryErrors)
    {
        primaryError = PrimaryError;
        secondaryErrors = SecondaryErrors;
    }
}
