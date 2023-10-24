using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Testing.Integration.Seeding;
using EasyDesk.CleanArchitecture.Testing.Integration.Services;
using EasyDesk.Commons.Tasks;
using EasyDesk.SampleApp.Application.V_1_0.IncomingCommands;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Seeders;

public class SampleTestData
{
    public static TenantId TestTenant { get; } = new("test-tenant");

    public int OperationsRun { get; set; } = 0;
}

public class SampleSeeder : WebServiceSeeder<SampleAppTestsFixture, SampleTestData>
{
    public SampleSeeder(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    public override async Task Seed(SampleTestData data)
    {
        var operationsRun = 0;

        async Task RunOperation(AsyncAction operation)
        {
            await operation();
            operationsRun++;
        }

        await RunOperation(() => DefaultBusEndpoint.Send(new CreateTenant(SampleTestData.TestTenant)));
        await WebService.WaitUntilTenantExists(SampleTestData.TestTenant);

        data.OperationsRun = 0;
    }
}
