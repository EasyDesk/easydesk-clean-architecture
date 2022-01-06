using System;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel;

public static class DomainConstraints
{
    public static void Require(bool requirement, Func<DomainError> error)
    {
        if (!requirement)
        {
            throw new DomainConstraintException(error());
        }
    }

    public static void RequireFalse(bool requirement, Func<DomainError> error) => Require(!requirement, error);
}
