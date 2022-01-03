using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.CleanArchitecture.Domain.Model;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using EasyDesk.Tools;
using System.Threading.Tasks;
using static EasyDesk.CleanArchitecture.Application.Responses.ResponseImports;

namespace EasyDesk.SampleApp.Application.Commands;

public static class CreatePerson
{
    public record Command(string Name) : CommandBase<Nothing>;

    public class Handler : UnitOfWorkHandler<Command, Nothing>
    {
        private readonly IPersonRepository _personRepository;

        public Handler(IPersonRepository personRepository, IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _personRepository = personRepository;
        }

        protected override Task<Response<Nothing>> HandleRequest(Command request)
        {
            var person = Person.Create(Name.From(request.Name));
            _personRepository.Save(person);
            return OkAsync;
        }
    }
}
