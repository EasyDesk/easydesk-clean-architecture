using EasyDesk.CleanArchitecture.Testing.Integration.Seeding;
using EasyDesk.CleanArchitecture.Testing.Integration.Session;
using EasyDesk.Commons.Tasks;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Seeders;

public class SampleSeeder : ISeeder<SampleAppTestsFixture, SampleSeeder.Data>
{
    public record Data(int OperationsRun);

    public void ConfigureSession(SessionConfigurer configurer)
    {
    }

    public async Task<Data> Seed(IntegrationTestSession<SampleAppTestsFixture> session)
    {
        var operationsRun = 0;

        async Task RunOperation(AsyncAction operation, int expectedOperations = 1)
        {
            await operation();
            operationsRun += expectedOperations;
        }

        return new(operationsRun);
    }
}
