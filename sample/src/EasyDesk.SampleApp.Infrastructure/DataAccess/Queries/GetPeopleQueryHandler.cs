using AutoMapper;
using AutoMapper.QueryableExtensions;
using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Application.Pages;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.SampleApp.Application.Queries;
using System.Linq;
using System.Threading.Tasks;
using static EasyDesk.SampleApp.Application.Queries.GetPeople;

namespace EasyDesk.SampleApp.Infrastructure.DataAccess.Queries;

public class GetPeopleQueryHandler : PaginatedQueryHandlerBase<Query, PersonSnapshot>
{
    private readonly SampleAppContext _context;
    private readonly IMapper _mapper;

    public GetPeopleQueryHandler(SampleAppContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    protected override async Task<Response<Page<PersonSnapshot>>> Handle(Query request)
    {
        return await _context.People
            .OrderBy(p => p.Name)
            .ProjectTo<PersonSnapshot>(_mapper.ConfigurationProvider)
            .GetPageAsync(request.Pagination);
    }
}
