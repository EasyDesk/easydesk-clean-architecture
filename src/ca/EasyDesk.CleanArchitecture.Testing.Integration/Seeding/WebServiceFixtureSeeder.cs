using EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;
using EasyDesk.CleanArchitecture.Testing.Integration.Sessions;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Seeding;

public abstract class WebServiceFixtureSeeder<TFixture> : WebServiceTestSession<TFixture>
    where TFixture : WebServiceTestsFixture<TFixture>
{
    protected WebServiceFixtureSeeder(TFixture fixture) : base(fixture)
    {
    }

    public abstract Task Seed();

    public abstract class WithData<TData> : WebServiceFixtureSeeder<TFixture>
        where TData : notnull
    {
        protected WithData(TFixture fixture) : base(fixture)
        {
        }

        public sealed override async Task Seed()
        {
            var data = await SeedWithData();
            Fixture.UpdateSeedingResult(data);
        }

        protected abstract Task<TData> SeedWithData();
    }
}
