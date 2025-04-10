using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Testing.Integration.Multitenancy;
using EasyDesk.CleanArchitecture.Testing.Integration.Seeding;
using EasyDesk.CleanArchitecture.Testing.Integration.Session;
using EasyDesk.Commons.Tasks;
using EasyDesk.SampleApp.Application.V_1_0.IncomingCommands;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Seeders;

public class SampleSeeder : ISeeder<SampleAppTestsFixture, SampleSeeder.Data>
{
    public record Data(int OperationsRun)
    {
        public static TenantId TestTenant { get; } = new("test-tenant");
    }

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

        await RunOperation(() => session.DefaultBusEndpoint.Send(new CreateTenant(Data.TestTenant)));
        await session.Host.WaitUntilTenantExists(Data.TestTenant);

        return new(operationsRun);
    }
}
