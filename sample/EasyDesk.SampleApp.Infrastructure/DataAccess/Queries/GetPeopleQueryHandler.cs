using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Pagination;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.SampleApp.Application.Queries;
using EasyDesk.SampleApp.Infrastructure.DataAccess.Model;

namespace EasyDesk.SampleApp.Infrastructure.DataAccess.Queries;

public class GetPeopleQueryHandler : IHandler<GetPeople, Pageable<PersonSnapshot>>
{
    private readonly SampleAppContext _context;

    public GetPeopleQueryHandler(SampleAppContext context)
    {
        _context = context;
    }

    public Task<Result<Pageable<PersonSnapshot>>> Handle(GetPeople query)
    {
        return Task.FromResult(Success(_context.People
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .Project<PersonModel, PersonSnapshot>()
            .ToPageable()));
    }
}
