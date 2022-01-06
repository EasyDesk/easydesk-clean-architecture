using System;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel;

public class DomainConstraintException : Exception
{
    public DomainConstraintException(DomainError error) : base($"Domain constraint violated. Error: {error}")
    {
        DomainError = error;
    }

    public DomainError DomainError { get; }
}
