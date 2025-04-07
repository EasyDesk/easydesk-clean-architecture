using EasyDesk.CleanArchitecture.Testing.Integration.Refactor.Fixture;
using EasyDesk.CleanArchitecture.Testing.Integration.Refactor.Session;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Refactor.Seeding;

public interface ISeeder<TFixture, TSeed>
    where TFixture : IntegrationTestsFixture
{
    void ConfigureSession(SessionConfigurer configurer);

    Task<TSeed> Seed(IntegrationTestSession<TFixture> session);
}
