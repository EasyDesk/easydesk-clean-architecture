using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.SampleApp.Domain.Aggregates.PetAggregate;

namespace EasyDesk.SampleApp.Application.IncomingEvents;

public record PetFreedomDayEvent(string Tenant) : IIncomingEvent, IOverrideMultitenantPolicy
{
    public MultitenantPolicy GetMultitenantPolicy() => MultitenantPolicies.IgnoreAndUseTenant(TenantId.Create(Tenant));
}

public class PetFreedomDayEventHandler : IHandler<PetFreedomDayEvent>
{
    private readonly IPetRepository _petRepository;

    public PetFreedomDayEventHandler(IPetRepository petRepository)
    {
        _petRepository = petRepository;
    }

    public async Task<Result<Nothing>> Handle(PetFreedomDayEvent request)
    {
        await _petRepository.RemoveAll();
        return Ok;
    }
}
