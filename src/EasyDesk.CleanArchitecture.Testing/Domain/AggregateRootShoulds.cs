using EasyDesk.CleanArchitecture.Domain.Metamodel;
using Shouldly;

namespace EasyDesk.CleanArchitecture.Testing.Domain
{
    [ShouldlyMethods]
    public static class AggregateRootShoulds
    {
        public static void ShouldHaveEmitted<T>(this T aggregateRoot, IDomainEvent expected)
            where T : AggregateRoot<T>
        {
            aggregateRoot.EmittedEvents.ShouldContain(expected);
        }
    }
}
