﻿using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.IntegrationTests.Seeders;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Paginated;
using EasyDesk.Commons.Options;
using EasyDesk.SampleApp.Application.V_1_0.AsyncCommands;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.People;
using NodaTime;
using Shouldly;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Commands;

public class CreateSiblingTests : SampleIntegrationTest
{
    private static readonly Duration _epsilon = Duration.FromSeconds(1);
    private static readonly Duration _waitTime = Duration.FromSeconds(5);
    private static readonly CreatePersonBodyDto _body = new()
    {
        FirstName = "Pippo",
        LastName = "Pluto",
        DateOfBirth = new(2000, 10, 11),
        Residence = AddressDto.Create("Some street"),
    };

    public CreateSiblingTests(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    protected override Option<TenantInfo> DefaultTenantInfo =>
        Some(TenantInfo.Tenant(SampleSeeder.Data.TestTenant));

    protected override Option<Agent> DefaultAgent => Some(TestAgents.Admin);

    protected override async Task OnInitialization()
    {
        await Http.AddAdmin().Send().EnsureSuccess();
    }

    [Fact]
    public async Task ShouldNotReceiveCommand_UntilScheduledTime()
    {
        await Http
            .CreatePerson(_body)
            .Send()
            .EnsureSuccess();

        Clock.Advance(CreateSibling.CreationDelay.Minus(_epsilon));

        await Task.Delay(_waitTime.ToTimeSpan());

        var data = await Http
            .GetPeople()
            .Send()
            .AsVerifiableEnumerable();

        data.ShouldNotContain(x => x.FirstName == "Pippo's sibling");
    }

    [Fact]
    public async Task ShouldReceiveCommand_AfterScheduledTime()
    {
        await Http
            .CreatePerson(_body)
            .Send()
            .EnsureSuccess();

        Clock.Advance(CreateSibling.CreationDelay.Plus(_epsilon));

        await Http
            .GetPeople()
            .PollUntil(people => people.Any(p => p.FirstName == "Pippo's sibling"))
            .EnsureSuccess();
    }
}
