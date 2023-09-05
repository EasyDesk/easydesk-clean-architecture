using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.Commons.Results;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using EasyDesk.SampleApp.Application.V_1_0.Queries;
using EasyDesk.SampleApp.Infrastructure.EfCore.Model;

namespace EasyDesk.SampleApp.Infrastructure.EfCore.Queries;

public class GetPersonQueryHandler : IHandler<GetPerson, PersonDto>
{
    private readonly SampleAppContext _context;

    public GetPersonQueryHandler(SampleAppContext context)
    {
        _context = context;
    }

    public async Task<Result<PersonDto>> Handle(GetPerson request)
    {
        return await _context.People
            .Where(p => p.Id == request.Id)
            .Project<PersonModel, PersonDto>()
            .FirstOptionAsync()
            .ThenOrElseError(Errors.NotFound);
    }
}
