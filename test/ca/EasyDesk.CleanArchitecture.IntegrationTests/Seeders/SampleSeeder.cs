using EasyDesk.CleanArchitecture.Testing.Integration.Seeding;
using EasyDesk.CleanArchitecture.Testing.Integration.Session;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Seeders;

public class SampleSeeder : ISeeder<SampleAppTestsFixture, SampleSeeder.Data>
{
    public record Data(int OperationsRun);

    public void ConfigureSession(SessionConfigurer configurer, SampleAppTestsFixture fixture)
    {
    }

    public async Task<Data> Seed(IntegrationTestSession<SampleAppTestsFixture> session, SampleAppTestsFixture fixture)
    {
        return new(0);
    }
}
