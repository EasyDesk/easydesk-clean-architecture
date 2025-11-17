using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Extensions;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Paginated;
using EasyDesk.CleanArchitecture.Testing.Integration.Multitenancy;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using EasyDesk.SampleApp.Application.V_1_0.IncomingCommands;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Commands;

public class IncomingCommandsTests : SampleAppIntegrationTest
{
    public IncomingCommandsTests(SampleAppTestsFixture factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateTenant_ShouldSucceed()
    {
        var tenantName = "test-tenant-asd";
        await Session.DefaultBusEndpoint.Send(new CreateTenant(tenantName));

        await Session.Host.WaitUntilTenantExists(new TenantId(tenantName));
    }

    [Fact]
    public async Task RemoveTenant_ShouldSucceed()
    {
        var tenantName = "test-tenant-qwe";
        await Session.DefaultBusEndpoint.Send(new CreateTenant(tenantName));

        var tenantId = new TenantId(tenantName);
        await Session.Host.WaitUntilTenantExists(tenantId);

        await Session.DefaultBusEndpoint.Send(new RemoveTenant(tenantName));

        await Session.Host.WaitUntilTenantDoesNotExist(tenantId);
    }

    [Fact]
    public async Task RemoveTenant_ShouldDeleteEntities()
    {
        var tenantId = new TenantId("test-tenant-qwe");
        await Session.DefaultBusEndpoint.Send(new CreateTenant(tenantId));

        using var adminScope = Session.AuthenticationManager.AuthenticateAs(TestAgents.Admin);
        using var tenantScope = Session.TenantManager.MoveToTenant(tenantId);

        await Session.Host.WaitUntilTenantExists(tenantId);
        await Session.Http.AddAdmin()
            .Send()
            .EnsureSuccess();

        var person = await Session.Http
            .CreatePerson(new()
            {
                FirstName = "Foo",
                LastName = "Bar",
                DateOfBirth = new(1996, 2, 2),
                Residence = AddressDto.Create("unknown"),
            })
            .Send()
            .AsData();

        await Session.Http
            .GetOwnedPets(person.Id)
            .PollUntil(pets => pets.Any())
            .EnsureSuccess();

        await Session.DefaultBusEndpoint.Send(new RemoveTenant(tenantId));
        await Session.Host.WaitUntilTenantDoesNotExist(tenantId);

        await Session.DefaultBusEndpoint.Send(new CreateTenant(tenantId));
        await Session.Host.WaitUntilTenantExists(tenantId);

        await Session.Http
            .GetOwnedPets(person.Id)
            .PollUntil(pets => !pets.Any())
            .EnsureSuccess();

        await Session.Http.GetPerson(person.Id)
            .PollWhile(w => w.IsSuccess())
            .Verify();
    }
}
