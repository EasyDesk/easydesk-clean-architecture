using AutoMapper;
using AutoMapper.QueryableExtensions;
using EasyDesk.CleanArchitecture.Application.Cqrs.Handlers;
using EasyDesk.CleanArchitecture.Application.Pagination;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.SampleApp.Application.Queries;
using static EasyDesk.SampleApp.Application.Queries.GetPeople;

namespace EasyDesk.SampleApp.Infrastructure.DataAccess.Queries;

public class GetPeopleQueryHandler : IQueryHandler<Query, Pageable<PersonSnapshot>>
{
    private readonly SampleAppContext _context;
    private readonly IMapper _mapper;

    public GetPeopleQueryHandler(SampleAppContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public Task<Result<Pageable<PersonSnapshot>>> Handle(Query query)
    {
        return Task.FromResult(Success(_context.People
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ProjectTo<PersonSnapshot>(_mapper.ConfigurationProvider)
            .ToPageable()));
    }
}
