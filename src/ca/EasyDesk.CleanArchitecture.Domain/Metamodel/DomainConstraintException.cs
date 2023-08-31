using EasyDesk.Commons.Collections;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel;

public sealed class DomainConstraintException : Exception
{
    public DomainConstraintException(params DomainError[] errors)
        : this(errors.AsEnumerable())
    {
    }

    public DomainConstraintException(IEnumerable<DomainError> errors)
        : base($"Domain constraints violated. Errors: {errors.ConcatStrings(",\n")}")
    {
        DomainErrors = errors;
    }

    public IEnumerable<DomainError> DomainErrors { get; }
}
