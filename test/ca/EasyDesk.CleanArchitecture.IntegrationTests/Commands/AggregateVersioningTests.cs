using Autofac;
using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Dal.EfCore.Domain;
using EasyDesk.CleanArchitecture.DependencyInjection;
using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.IntegrationTests.Seeders;
using EasyDesk.Commons.Options;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using EasyDesk.SampleApp.Infrastructure.EfCore;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Commands;

public class AggregateVersioningTests : SampleIntegrationTest
{
    private PersonDto _person = default!;

    public AggregateVersioningTests(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    protected override Option<TenantInfo> DefaultTenantInfo =>
        Some(TenantInfo.Tenant(SampleSeeder.Data.TestTenant));

    protected override Option<Agent> DefaultAgent => Some(TestAgents.Admin);

    protected override async Task OnInitialization()
    {
        await Http.AddAdmin().Send().EnsureSuccess();

        _person = await Http
            .CreatePerson(new()
            {
                FirstName = "Alan",
                LastName = "Turing",
                DateOfBirth = new(1990, 12, 02),
                Residence = AddressDto.Create("street"),
            })
            .Send()
            .AsData();
    }

    private async Task<long> GetVersion()
    {
        await using var scope = WebService.LifetimeScope.BeginUseCaseLifetimeScope();
        var context = scope.Resolve<SampleAppContext>();
        scope.Resolve<IContextTenantInitializer>().Initialize(DefaultTenantInfo.Value);
        return await context.People
            .Where(x => x.Id == _person.Id)
            .Select(x => EF.Property<long>(x, AggregateVersioningUtils.VersionPropertyName))
            .FirstAsync();
    }

    [Fact]
    public async Task CreatingPerson_ShouldInitializeVersion()
    {
        var version = await GetVersion();
        version.ShouldBe(2);
    }

    [Fact]
    public async Task UpdatingPerson_ShouldInitializeVersion()
    {
        await Http
            .UpdatePerson(_person.Id, new(
                FirstName: "Alan",
                LastName: "Smith",
                Residence: AddressDto.Create("New street")))
            .Send()
            .EnsureSuccess();

        var version = await GetVersion();
        version.ShouldBe(3);
    }
}
