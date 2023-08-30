using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Paginated;
using EasyDesk.CleanArchitecture.Testing.Integration.Services;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using EasyDesk.SampleApp.Application.V_1_0.IncomingCommands;
using NodaTime;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Commands;

public class IncomingCommandsTests : SampleIntegrationTest
{
    public IncomingCommandsTests(SampleAppTestsFixture factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateTenant_ShouldSucceed()
    {
        var tenantName = "test-tenant-asd";
        await DefaultBusEndpoint.Send(new CreateTenant(tenantName));

        await WebService.WaitUntilTenantExists(TenantId.New(tenantName));
    }

    [Fact]
    public async Task RemoveTenant_ShouldSucceed()
    {
        var tenantName = "test-tenant-qwe";
        await DefaultBusEndpoint.Send(new CreateTenant(tenantName));

        var tenantId = TenantId.New(tenantName);
        await WebService.WaitUntilTenantExists(tenantId);

        await DefaultBusEndpoint.Send(new RemoveTenant(tenantName));

        await WebService.WaitUntilTenantDoesNotExist(tenantId);
    }

    [Fact]
    public async Task RemoveTenant_ShouldDeleteEntities()
    {
        var tenantId = TenantId.New("test-tenant-qwe");
        await DefaultBusEndpoint.Send(new CreateTenant(tenantId));

        AuthenticateAs(TestAgents.Admin);
        TenantNavigator.MoveToTenant(tenantId);

        await WebService.WaitUntilTenantExists(tenantId);
        await Http.AddAdmin()
            .Send()
            .EnsureSuccess();

        var person = await Http
            .CreatePerson(new(
                FirstName: "Foo",
                LastName: "Bar",
                DateOfBirth: new LocalDate(1996, 2, 2),
                Residence: AddressDto.Create("unknown")))
            .Send()
            .AsData();

        await Http
            .GetOwnedPets(person.Id)
            .PollUntil(pets => pets.Any())
            .EnsureSuccess();

        await DefaultBusEndpoint.Send(new RemoveTenant(tenantId));
        await WebService.WaitUntilTenantDoesNotExist(tenantId);

        await DefaultBusEndpoint.Send(new CreateTenant(tenantId));
        await WebService.WaitUntilTenantExists(tenantId);

        await Http
            .GetOwnedPets(person.Id)
            .PollUntil(pets => !pets.Any())
            .EnsureSuccess();

        var response = await Http.GetPerson(person.Id)
            .PollWhile(w => w.IsSuccess())
            .AsVerifiable();

        await Verify(response);
    }
}
