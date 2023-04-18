using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Pagination;
using EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces.Abstractions;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.SampleApp.Application.Queries;
using EasyDesk.SampleApp.Application.Snapshots;
using EasyDesk.SampleApp.Infrastructure.EfCore.Model;

namespace EasyDesk.SampleApp.Infrastructure.EfCore.Queries;

public class GetPeopleQueryHandler : SuccessHandler<GetPeople, IPageable<PersonSnapshot>>
{
    private readonly SampleAppContext _context;

    public GetPeopleQueryHandler(SampleAppContext context)
    {
        _context = context;
    }

    protected override Task<IPageable<PersonSnapshot>> Process(GetPeople query)
    {
        return Task.FromResult(_context.People
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .Project<PersonModel, PersonSnapshot>()
            .ToPageable());
    }
}
