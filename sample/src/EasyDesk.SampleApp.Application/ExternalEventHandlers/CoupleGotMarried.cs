using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Events.ExternalEvents;
using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using EasyDesk.Tools;
using System;
using System.Threading.Tasks;
using static EasyDesk.CleanArchitecture.Application.Responses.ResponseImports;

namespace EasyDesk.SampleApp.Application.ExternalEventHandlers
{
    public record CoupleGotMarried(Guid GroomId, Guid BrideId) : ExternalEvent;

    public class CoupleGotMarriedHandler : ExternalEventHandlerBase<CoupleGotMarried>
    {
        private readonly IPersonRepository _personRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CoupleGotMarriedHandler(IPersonRepository personRepository, IUnitOfWork unitOfWork)
        {
            _personRepository = personRepository;
            _unitOfWork = unitOfWork;
        }

        protected override async Task<Response<Nothing>> Handle(CoupleGotMarried ev)
        {
            return await SetPersonAsMarried(ev.GroomId)
                .ThenRequireAsync(_ => SetPersonAsMarried(ev.BrideId))
                .ThenRequireAsync(_ => _unitOfWork.Save());
        }

        private async Task<Response<Nothing>> SetPersonAsMarried(Guid personId)
        {
            var person = await _personRepository.GetById(personId);
            return person.Match(
                some: person => person.Marry().ToResponse(),
                none: () => Ok);
        }
    }
}
