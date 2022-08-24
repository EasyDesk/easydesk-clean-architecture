using EasyDesk.Tools.Collections;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel;

public class DomainConstraintException : Exception
{
    public DomainConstraintException(IEnumerable<DomainError> errors) : base($"Domain constraints violated. Errors: {errors.ConcatStrings(",\n")}")
    {
        DomainErrors = errors;
    }

    public IEnumerable<DomainError> DomainErrors { get; }
}
