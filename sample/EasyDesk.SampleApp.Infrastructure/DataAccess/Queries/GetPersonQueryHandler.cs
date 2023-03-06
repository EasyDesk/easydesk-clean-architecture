using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces.Abstractions;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.SampleApp.Application.Queries;
using EasyDesk.SampleApp.Application.Snapshots;
using EasyDesk.SampleApp.Infrastructure.DataAccess.Model;

namespace EasyDesk.SampleApp.Infrastructure.DataAccess.Queries;

public class GetPersonQueryHandler : IHandler<GetPerson, PersonSnapshot>
{
    private readonly SampleAppContext _context;

    public GetPersonQueryHandler(SampleAppContext context)
    {
        _context = context;
    }

    public async Task<Result<PersonSnapshot>> Handle(GetPerson request)
    {
        return await _context.People
            .Where(p => p.Id == request.Id)
            .Project<PersonModel, PersonSnapshot>()
            .FirstOptionAsync()
            .ThenOrElseError(Errors.NotFound);
    }
}
