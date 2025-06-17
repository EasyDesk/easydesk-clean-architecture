using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Testing.Unit.Domain;

namespace EasyDesk.CleanArchitecture.UnitTests.Domain.Metamodel;

public class AggregateRootTests
{
    private record Event(int Value) : DomainEvent;

    private class TestAggregateRoot : AggregateRoot
    {
        protected override void OnCreation() => EmitEvent(ToEvent(1));

        protected override void OnRemoval() => EmitEvent(ToEvent(2));
    }

    private readonly TestAggregateRoot _sut = new();

    private static Event ToEvent(int value) => new(value);

    [Fact]
    public void NotifyCreation_ShouldEmitEvent()
    {
        _sut.NotifyCreation();
        _sut.ShouldHaveEmitted(ToEvent(1));
        _sut.ShouldNotHaveEmitted(ToEvent(2));
    }

    [Fact]
    public void NotifyRemoval_ShouldEmitEvent()
    {
        _sut.NotifyRemoval();
        _sut.ShouldHaveEmitted(ToEvent(2));
        _sut.ShouldNotHaveEmitted(ToEvent(1));
    }

    [Fact]
    public void EmittedEvents_ShouldStartEmpty()
    {
        _sut.ShouldNotHaveEmitted<Event>();
    }
}
