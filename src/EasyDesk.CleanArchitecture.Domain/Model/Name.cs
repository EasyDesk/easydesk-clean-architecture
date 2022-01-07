using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Values;

namespace EasyDesk.CleanArchitecture.Domain.Model;

public record Name : ValueWrapper<string, Name>
{
    private Name(string name) : base(name.Trim())
    {
        DomainConstraints.Check()
            .IfNot(string.IsNullOrWhiteSpace(Value), () => new EmptyName())
            .OtherwiseThrowException();
    }

    public static Name From(string value) => new(value);
}

public record EmptyName : DomainError;
