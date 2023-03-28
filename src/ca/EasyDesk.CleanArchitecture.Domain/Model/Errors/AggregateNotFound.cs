using EasyDesk.CleanArchitecture.Domain.Metamodel;

namespace EasyDesk.CleanArchitecture.Domain.Model.Errors;

public record AggregateNotFound(string AggregateType) : DomainError
{
    public static AggregateNotFound OfType<T>() where T : AggregateRoot => new(typeof(T).Name);
}
