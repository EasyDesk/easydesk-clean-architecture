using EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;
using EasyDesk.CleanArchitecture.Testing.Integration.Sessions;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Seeding;

public abstract class WebServiceSeeder<TFixture, TData> : WebServiceTestSession<TFixture>, ISeeder<TData>
    where TFixture : WebServiceTestsFixture
{
    protected WebServiceSeeder(TFixture fixture) : base(fixture)
    {
    }

    public abstract Task<TData> Seed();
}
