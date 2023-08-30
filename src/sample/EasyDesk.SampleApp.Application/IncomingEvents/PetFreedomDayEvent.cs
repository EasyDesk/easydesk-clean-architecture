using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.Commons.Results;
using EasyDesk.SampleApp.Domain.Aggregates.PetAggregate;

namespace EasyDesk.SampleApp.Application.IncomingEvents;

public record PetFreedomDayEvent : IIncomingEvent;

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
