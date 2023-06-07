using EasyDesk.CleanArchitecture.Domain.Metamodel;
using Shouldly;

namespace EasyDesk.CleanArchitecture.Testing.Unit.Domain;

[ShouldlyMethods]
public static class AggregateRootShoulds
{
    public static void ShouldHaveEmitted(this Entity entity, DomainEvent expected)
    {
        entity.EmittedEvents().ShouldContain(expected);
    }
}
