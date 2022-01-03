using EasyDesk.CleanArchitecture.Domain.Metamodel;
using System;

namespace EasyDesk.CleanArchitecture.Domain.Model.Errors;

public record AggregateNotFound(Type AggregateType) : DomainError
{
    public static AggregateNotFound OfType<T>() => new(typeof(T));
}
