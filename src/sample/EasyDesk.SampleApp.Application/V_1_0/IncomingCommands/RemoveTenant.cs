using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using EasyDesk.SampleApp.Domain.Aggregates.PetAggregate;

namespace EasyDesk.SampleApp.Application.V_1_0.IncomingCommands;

public record RemoveTenant(string Id) : IIncomingCommand, IOverrideMultitenantPolicy
{
    public MultitenantPolicy GetMultitenantPolicy() => MultitenantPolicies.IgnoreAndUseTenant(TenantId.New(Id));
}

public class RemoveTenantHandler : IHandler<RemoveTenant>
{
    private readonly IMultitenancyManager _multitenancyManager;
    private readonly ITenantProvider _tenantProvider;
    private readonly IPersonRepository _personRepository;
    private readonly IPetRepository _petRepository;

    public RemoveTenantHandler(
        IMultitenancyManager multitenancyManager,
        ITenantProvider tenantProvider,
        IPersonRepository personRepository,
        IPetRepository petRepository)
    {
        _multitenancyManager = multitenancyManager;
        _tenantProvider = tenantProvider;
        _personRepository = personRepository;
        _petRepository = petRepository;
    }

    public async Task<Result<Nothing>> Handle(RemoveTenant request)
    {
        await _petRepository.RemoveAll();
        await _personRepository.RemoveAll();
        await _multitenancyManager.RemoveTenant(_tenantProvider.Tenant.Id.Value);
        return Ok;
    }
}
