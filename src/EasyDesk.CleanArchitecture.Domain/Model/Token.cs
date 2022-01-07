using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Values;
using System;

namespace EasyDesk.CleanArchitecture.Domain.Model;

public record Token : ValueWrapper<string, Token>
{
    private Token(string value) : base(value)
    {
        DomainConstraints.Check()
            .IfNot(string.IsNullOrWhiteSpace(value), () => new EmptyToken())
            .ThrowException();
    }

    public static Token From(string value) => new(value);

    public static Token Random() => From(Guid.NewGuid().ToString());
}

public record EmptyToken : DomainError;
