using EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;
using EasyDesk.CleanArchitecture.Testing.Integration.Sessions;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Seeding;

public abstract class WebServiceSeeder<T> : WebServiceTestSession<T>, ISeeder
    where T : WebServiceTestsFixture
{
    protected WebServiceSeeder(T fixture) : base(fixture)
    {
    }

    public abstract Task Seed();
}
