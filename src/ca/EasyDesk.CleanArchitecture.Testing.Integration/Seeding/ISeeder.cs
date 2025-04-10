using EasyDesk.CleanArchitecture.Testing.Integration.Fixture;
using EasyDesk.CleanArchitecture.Testing.Integration.Session;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Seeding;

public interface ISeeder<TFixture, TSeed>
    where TFixture : IntegrationTestsFixture
{
    void ConfigureSession(SessionConfigurer configurer);

    Task<TSeed> Seed(IntegrationTestSession<TFixture> session);
}
