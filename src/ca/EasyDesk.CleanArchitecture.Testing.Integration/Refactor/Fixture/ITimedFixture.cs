using NodaTime.Testing;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Refactor.Fixture;

public interface ITimedFixture
{
    FakeClock Clock { get; }
}
