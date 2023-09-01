using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Testing.Unit.Application;
using EasyDesk.CleanArchitecture.Testing.Unit.Commons;
using EasyDesk.SampleApp.Application.V_1_0.Commands;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using EasyDesk.SampleApp.Domain.Aggregates.PetAggregate;
using EasyDesk.Testing.Assertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using static EasyDesk.CleanArchitecture.Testing.Unit.Domain.IFindByIdRepositorySubstituteUtils;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Handlers;

public class CreatePetHandlerTests : RequestHandlerTestsBase<CreatePetHandler, CreatePet, PetDto>
{
    private readonly IPersonRepository _personRepository = Substitute.For<IPersonRepository>();
    private readonly IPetRepository _petRepository = Substitute.For<IPetRepository>();
    private readonly IAuditConfigurer _auditConfigurer = Substitute.For<IAuditConfigurer>();
    private readonly ITenantNavigator _tenantNavigator = new TestTenantNavigator();
    private readonly Guid _personId = Guid.Parse("ace69604-d811-4c6b-8440-ba968a9b8314");

    public CreatePetHandlerTests()
    {
        _personRepository.FindById(default!).ReturnsForAnyArgs(None);
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services.AddSingleton(_personRepository);
        services.AddSingleton(_petRepository);
        services.AddSingleton(_auditConfigurer);
        services.AddSingleton(_tenantNavigator);
    }

    [Fact]
    public async Task ShouldReturnNotFound_IfPersonWasntFound()
    {
        var result = await Handle(new(
            new("fufi"),
            PersonId: _personId));
        result.ShouldBeFailure();
        result.ReadError().ShouldBe(Errors.NotFound());
    }

    [Fact]
    public async Task ShouldSucceedIfPersonIsFound()
    {
        _personRepository.FindById(_personId).Returns(new Person(
            _personId,
            new("Mario"),
            new("Rossi"),
            new(2000, 1, 1),
            AdminId.From("asd"),
            new(
                StreetType: None,
                StreetName: new("xd"),
                StreetNumber: None,
                None,
                None,
                None,
                None,
                None,
                None),
            true));
        var result = await Handle(new(
            new("fufi"),
            PersonId: _personId));
        result.ShouldBeSuccess();
        var pet = result.ReadValue();
        pet.Nickname.ShouldBe("fufi");
    }
}
