using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Events.ExternalEvents;
using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Results;
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

        public CoupleGotMarriedHandler(IPersonRepository personRepository, IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _personRepository = personRepository;
        }

        protected override async Task<Response<Nothing>> Handle(CoupleGotMarried ev)
        {
            return await SetPersonAsMarried(ev.GroomId)
                .ThenRequireAsync(_ => SetPersonAsMarried(ev.BrideId));
        }

        private async Task<Response<Nothing>> SetPersonAsMarried(Guid personId)
        {
            return await _personRepository.GetById(personId)
                .ThenRequire(person => person.Marry())
                .ThenIfSuccess(_personRepository.Save)
                .ThenToResponse();
        }
    }
}
