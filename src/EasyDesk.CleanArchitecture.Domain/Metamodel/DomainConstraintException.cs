using EasyDesk.Tools.Collections;
using System;
using System.Collections.Generic;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel;

public class DomainConstraintException : Exception
{
    public DomainConstraintException(IEnumerable<DomainError> errors) : base($"Domain constraints violated. Errors: {errors.ConcatStrings(",\n")}")
    {
        DomainErrors = errors;
    }

    public IEnumerable<DomainError> DomainErrors { get; }
}
