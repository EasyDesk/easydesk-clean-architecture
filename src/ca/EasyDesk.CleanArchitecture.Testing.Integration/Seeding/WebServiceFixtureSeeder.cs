using EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;
using EasyDesk.CleanArchitecture.Testing.Integration.Sessions;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Seeding;

public abstract class WebServiceFixtureSeeder<TFixture, TSeed> : WebServiceTestSession<TFixture>
    where TFixture : ITestFixture
{
    protected WebServiceFixtureSeeder(TFixture fixture) : base(fixture)
    {
    }

    public abstract Task<TSeed> Seed();
}
