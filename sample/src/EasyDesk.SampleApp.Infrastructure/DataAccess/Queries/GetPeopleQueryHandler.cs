using AutoMapper;
using AutoMapper.QueryableExtensions;
using EasyDesk.CleanArchitecture.Application.Mediator.Handlers;
using EasyDesk.CleanArchitecture.Application.Pages;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.SampleApp.Application.Queries;
using static EasyDesk.SampleApp.Application.Queries.GetPeople;

namespace EasyDesk.SampleApp.Infrastructure.DataAccess.Queries;

public class GetPeopleQueryHandler : IQueryWithPaginationHandler<Query, PersonSnapshot>
{
    private readonly SampleAppContext _context;
    private readonly IMapper _mapper;

    public GetPeopleQueryHandler(SampleAppContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<Page<PersonSnapshot>>> Handle(Query request, CancellationToken cancellationToken)
    {
        return await _context.People
            .OrderBy(p => p.Name)
            .ProjectTo<PersonSnapshot>(_mapper.ConfigurationProvider)
            .GetPageAsync(request.Pagination)
            .MapPage(p => p);
    }
}
