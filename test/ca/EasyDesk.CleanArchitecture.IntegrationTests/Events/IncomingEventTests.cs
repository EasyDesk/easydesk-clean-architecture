using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.IntegrationTests.Seeders;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Paginated;
using EasyDesk.Commons.Collections;
using EasyDesk.SampleApp.Application.IncomingEvents;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using NodaTime;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Events;

public class IncomingEventTests : SampleIntegrationTest
{
    private PersonDto? _person;

    public IncomingEventTests(SampleAppTestsFixture factory) : base(factory)
    {
    }

    protected override async Task OnInitialization()
    {
        TenantNavigator.MoveToTenant(SampleSeeder.Data.TestTenant);
        AuthenticateAs(TestAgents.Admin);

        await Http.AddAdmin().Send().EnsureSuccess();

        _person = await Http
            .CreatePerson(new(
                FirstName: "Foo",
                LastName: "Bar",
                DateOfBirth: new LocalDate(1995, 10, 12),
                Residence: AddressDto.Create("somewhere")))
            .Send()
            .AsData();

        await Http
            .CreatePet(_person.Id, new("Snoopy"))
            .Send()
            .EnsureSuccess();

        await Http.GetOwnedPets(_person.Id).PollUntil(pets => pets.Count() == 2).EnsureSuccess();
    }

    [Fact]
    public async Task PetFreedomDayIncomingEvent_ShouldSucceed()
    {
        var bus = NewBusEndpoint("pet-freedom-service");
        TenantNavigator.MoveToTenant(SampleSeeder.Data.TestTenant);
        await bus.Publish(new PetFreedomDayEvent());
        await Http.GetOwnedPets(_person!.Id).PollUntil(pets => pets.IsEmpty()).EnsureSuccess();
    }
}
