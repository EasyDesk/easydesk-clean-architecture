using EasyDesk.CleanArchitecture.Domain.Metamodel;

namespace EasyDesk.CleanArchitecture.Testing;

public record TestDomainError(string Value) : DomainError
{
    public static TestDomainError Create() => Create("DOMAIN_ERROR");

    public static TestDomainError Create(string value) => new(value);
}
