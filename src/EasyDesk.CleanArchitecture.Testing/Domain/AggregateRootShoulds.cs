using EasyDesk.CleanArchitecture.Domain.Metamodel;
using Shouldly;

namespace EasyDesk.CleanArchitecture.Testing.Domain
{
    [ShouldlyMethods]
    public static class AggregateRootShoulds
    {
        public static void ShouldHaveEmitted(this AggregateRoot aggregateRoot, IDomainEvent expected)
        {
            aggregateRoot.EmittedEvents.ShouldContain(expected);
        }
    }
}
