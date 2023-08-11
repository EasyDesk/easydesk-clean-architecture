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

    public static void ShouldNotHaveEmitted(this Entity entity, DomainEvent expected)
    {
        entity.EmittedEvents().ShouldNotContain(expected);
    }

    public static void ShouldNotHaveEmitted<T>(this Entity entity) where T : DomainEvent
    {
        entity.EmittedEvents().ShouldNotContain(e => e is T);
    }
}
