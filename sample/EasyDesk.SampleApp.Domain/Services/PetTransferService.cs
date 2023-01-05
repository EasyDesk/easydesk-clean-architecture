using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using EasyDesk.SampleApp.Domain.Aggregates.PetAggregate;

namespace EasyDesk.SampleApp.Domain.Services;

public class PetTransferService
{
    private readonly IPersonRepository _personRepository;
    private readonly IPetRepository _petRepository;

    public PetTransferService(IPersonRepository personRepository, IPetRepository petRepository)
    {
        _personRepository = personRepository;
        _petRepository = petRepository;
    }

    public async Task TransferPet(Guid from, Guid to, int petId)
    {
        var fromPersonExists = await _personRepository.Exists(from);
        var toPersonExists = await _personRepository.Exists(to);
        if (fromPersonExists && toPersonExists)
        {
            var pet = await _petRepository.GetById(petId);
            await pet.IfPresentAsync(async p =>
            {
                p.ChangeOwner(to);
                await _petRepository.SaveAndHydrate(p);
            });
        }
    }
}
