using EasyDesk.Commons.Collections;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel;

public sealed class DomainConstraints
{
    private readonly List<DomainError> _errors = new();

    private DomainConstraints()
    {
    }

    public DomainConstraints If(bool requirement, Func<DomainError> error)
    {
        if (requirement)
        {
            _errors.Add(error());
        }
        return this;
    }

    public DomainConstraints ElseIf(bool requirement, Func<DomainError> error) => If(_errors.IsEmpty() && requirement, error);

    public DomainConstraints IfNot(bool requirement, Func<DomainError> error) => If(!requirement, error);

    public DomainConstraints ElseIfNot(bool requirement, Func<DomainError> error) => If(_errors.IsEmpty() && !requirement, error);

    public void ThrowException()
    {
        if (_errors.Any())
        {
            throw new DomainConstraintException(_errors);
        }
    }

    public static implicit operator bool(DomainConstraints builder) => builder._errors.IsEmpty();

    public static DomainConstraints Check() => new();
}
