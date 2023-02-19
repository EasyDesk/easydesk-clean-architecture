using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Values;

namespace EasyDesk.CleanArchitecture.Domain.Model;

public record Name : ValueWrapper<string, Name>
{
    public Name(string name) : base(name.Trim())
    {
        DomainConstraints.Check()
            .If(string.IsNullOrWhiteSpace(Value), () => new EmptyName())
            .ThrowException();
    }
}

public record EmptyName : DomainError;
