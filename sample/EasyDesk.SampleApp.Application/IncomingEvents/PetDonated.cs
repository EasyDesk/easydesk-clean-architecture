using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.SampleApp.Domain.Services;

namespace EasyDesk.SampleApp.Application.IncomingEvents;

public record PetDonated(string Tenant, Guid From, Guid To, int PetId) : IIncomingEvent, IOverrideMultitenantPolicy
{
    public MultitenantPolicy GetMultitenantPolicy() => MultitenantPolicies.IgnoreAndUseTenant(TenantId.Create(Tenant));
}

public class PetDonatedHandler : IHandler<PetDonated>
{
    private readonly PetTransferService _petTransferService;

    public PetDonatedHandler(PetTransferService petTransferService)
    {
        _petTransferService = petTransferService;
    }

    public async Task<Result<Nothing>> Handle(PetDonated request)
    {
        await _petTransferService.TransferPet(request.From, request.To, request.PetId);
        return Ok;
    }
}
