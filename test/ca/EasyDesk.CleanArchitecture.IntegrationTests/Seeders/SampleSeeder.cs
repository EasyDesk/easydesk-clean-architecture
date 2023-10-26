using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Testing.Integration.Seeding;
using EasyDesk.CleanArchitecture.Testing.Integration.Services;
using EasyDesk.Commons.Tasks;
using EasyDesk.SampleApp.Application.V_1_0.IncomingCommands;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Seeders;

public class SampleSeeder : WebServiceFixtureSeeder<SampleAppTestsFixture>.WithData<SampleSeeder.Data>
{
    public record Data(int OperationsRun)
    {
        public static TenantId TestTenant { get; } = new("test-tenant");
    }

    public SampleSeeder(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    protected override async Task<Data> SeedWithData()
    {
        var operationsRun = 0;

        async Task RunOperation(AsyncAction operation, int expectedOperations = 1)
        {
            await operation();
            operationsRun += expectedOperations;
        }

        await RunOperation(() => DefaultBusEndpoint.Send(new CreateTenant(Data.TestTenant)));
        await WebService.WaitUntilTenantExists(Data.TestTenant);

        return new(operationsRun);
    }
}
