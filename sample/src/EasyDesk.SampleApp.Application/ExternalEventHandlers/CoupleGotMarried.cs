using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using EasyDesk.Tools;
using EasyDesk.Tools.Results;
using System;
using System.Threading.Tasks;
using static EasyDesk.Tools.Results.ResultImports;

namespace EasyDesk.SampleApp.Application.ExternalEventHandlers;

public record CoupleGotMarried(Guid GroomId, Guid BrideId) : IMessage;

public class CoupleGotMarriedHandler : IMessageHandler<CoupleGotMarried>
{
    private readonly IPersonRepository _personRepository;

    public CoupleGotMarriedHandler(IPersonRepository personRepository)
    {
        _personRepository = personRepository;
    }

    public async Task<Result<Nothing>> Handle(CoupleGotMarried ev)
    {
        return await SetPersonAsMarried(ev.GroomId)
            .ThenFlatTapAsync(_ => SetPersonAsMarried(ev.BrideId));
    }

    private async Task<Result<Nothing>> SetPersonAsMarried(Guid personId)
    {
        return await _personRepository.GetById(personId)
            .ThenFlatTap(person => person.Marry())
            .ThenIfSuccess(_personRepository.Save);
    }
}
