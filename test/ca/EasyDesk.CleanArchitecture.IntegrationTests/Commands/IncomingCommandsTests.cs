﻿using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.Testing.Integration.Services;
using EasyDesk.SampleApp.Application.IncomingCommands;
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
        var bus = NewBus();
        await bus.Send(new CreateTenant(tenantName));

        await WebService.WaitUntilTenantExists(TenantId.Create(tenantName));
    }

    [Fact]
    public async Task RemoveTenant_ShouldSucceed()
    {
        var tenantName = "test-tenant-qwe";
        var bus = NewBus();
        await bus.Send(new CreateTenant(tenantName));

        var tenantId = TenantId.Create(tenantName);
        await WebService.WaitUntilTenantExists(tenantId);

        await bus.Send(new RemoveTenant(tenantName));

        await WebService.WaitUntilTenantDoesNotExist(tenantId);
    }

    [Fact]
    public async Task RemoveTenant_ShouldDeleteEntities()
    {
        var tenantName = "test-tenant-qwe";
        var bus = NewBus();
        await bus.Send(new CreateTenant(tenantName));

        var tenantId = TenantId.Create(tenantName);
        await WebService.WaitUntilTenantExists(tenantId);
        var adminId = "test-admin-asd";
        await Http.AddAdmin()
            .AuthenticateAs(adminId)
            .Tenant(tenantName)
            .Send()
            .EnsureSuccess();

        var person = await Http
            .CreatePerson(new(
                FirstName: "Foo",
                LastName: "Bar",
                DateOfBirth: new LocalDate(1996, 2, 2),
                Residence: new("unknown")))
            .Tenant(tenantName)
            .AuthenticateAs(adminId)
            .Send()
            .AsData();

        await Http
            .GetOwnedPets(person.Id)
            .Tenant(tenantName)
            .AuthenticateAs(adminId)
            .PollUntil(pets => pets.Any())
            .EnsureSuccess();

        await bus.Send(new RemoveTenant(tenantName));
        await WebService.WaitUntilTenantDoesNotExist(tenantId);

        await bus.Send(new CreateTenant(tenantName));
        await WebService.WaitUntilTenantExists(tenantId);

        await Http
            .GetOwnedPets(person.Id)
            .Tenant(tenantName)
            .AuthenticateAs(adminId)
            .PollUntil(pets => !pets.Any())
            .EnsureSuccess();

        var response = await Http.GetPerson(person.Id)
            .Tenant(tenantName)
            .AuthenticateAs(adminId)
            .PollWhile(w => w.IsSuccess())
            .AsVerifiable();

        await Verify(response);
    }
}